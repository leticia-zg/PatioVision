using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using PatioVision.Core.Enums;
using PatioVision.Core.Models;
using PatioVision.Core.Models.ML;
using PatioVision.Data.Context;

namespace PatioVision.Service.Services.ML;

/// <summary>
/// Serviço para treinamento e predição de redistribuição de motos usando ML.NET
/// </summary>
public class RedistribuicaoMLService
{
    private readonly AppDbContext _context;
    private ITransformer? _trainedModel;
    private MLContext? _mlContext;
    private bool _isModelTrained = false;

    public RedistribuicaoMLService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Treina o modelo ML com dados históricos do banco
    /// </summary>
    public async Task TreinarModeloAsync()
    {
        Console.WriteLine("Iniciando treinamento do modelo ML...");

        _mlContext = new MLContext(seed: 0);
        
        // Buscar dados do banco para treinamento (sem Include para evitar ciclos)
        var patios = await _context.Patios
            .AsNoTracking()
            .ToListAsync();

        var motos = await _context.Motos
            .Include(m => m.Patio)
            .AsNoTracking()
            .ToListAsync();

        // Buscar contagem de motos por pátio separadamente
        var patioIdsList = patios.Select(p => p.PatioId).ToList();
        var quantidadeMotosPorPatio = await _context.Motos
            .Where(m => patioIdsList.Contains(m.PatioId) && m.Status == StatusMoto.Disponivel)
            .GroupBy(m => m.PatioId)
            .Select(g => new { PatioId = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.PatioId, x => x.Quantidade);

        if (!patios.Any() || !motos.Any())
        {
            throw new InvalidOperationException("Não há dados suficientes no banco para treinar o modelo. Execute o seeder primeiro.");
        }

        // Gerar dados de treinamento sintéticos baseados na distribuição atual
        var trainingData = GerarDadosTreinamento(patios, motos, quantidadeMotosPorPatio);

        Console.WriteLine($"Gerados {trainingData.Count} registros de treinamento");

        // Converter para IDataView
        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        // Dividir em treino e teste (80/20)
        var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

        // Configurar pipeline de treinamento
        // Concatenar todas as features em uma única coluna "Features"
        var featureColumns = new[]
        {
            nameof(RedistribuicaoInput.PatioAtualId),
            nameof(RedistribuicaoInput.PatioDestinoId),
            nameof(RedistribuicaoInput.QuantidadeMotosAtual),
            nameof(RedistribuicaoInput.QuantidadeMotosDestino),
            nameof(RedistribuicaoInput.CategoriaPatioAtual),
            nameof(RedistribuicaoInput.CategoriaPatioDestino),
            nameof(RedistribuicaoInput.StatusMoto),
            nameof(RedistribuicaoInput.DistanciaKm),
            nameof(RedistribuicaoInput.TaxaOcupacaoAtual),
            nameof(RedistribuicaoInput.TaxaOcupacaoDestino),
            nameof(RedistribuicaoInput.DiferencaEquilibrio),
            nameof(RedistribuicaoInput.MediaGeralMotosPorPatio)
        };

        var pipeline = _mlContext.Transforms.Concatenate("Features", featureColumns)
            .Append(_mlContext.Regression.Trainers.FastTree(
                labelColumnName: "Label",
                featureColumnName: "Features",
                numberOfLeaves: 20,
                numberOfTrees: 100,
                minimumExampleCountPerLeaf: 10,
                learningRate: 0.2
            ));

        // Treinar modelo
        Console.WriteLine("Treinando modelo...");
        _trainedModel = pipeline.Fit(split.TrainSet);

        // Avaliar modelo
        var predictions = _trainedModel.Transform(split.TestSet);
        var metrics = _mlContext.Regression.Evaluate(predictions, labelColumnName: "Label");

        Console.WriteLine($"R² Score: {metrics.RSquared:F4}");
        Console.WriteLine($"MAE: {metrics.MeanAbsoluteError:F4}");
        Console.WriteLine("Modelo treinado com sucesso!");

        _isModelTrained = true;
    }

    /// <summary>
    /// Gera recomendações de redistribuição para motos especificadas ou todas disponíveis
    /// </summary>
    public async Task<List<RedistribuicaoOutput>> GerarRecomendacoesAsync(
        List<Guid>? motoIds = null,
        List<Guid>? patioIds = null)
    {
        if (!_isModelTrained || _trainedModel == null || _mlContext == null)
        {
            await TreinarModeloAsync();
        }

        // Buscar motos disponíveis (Status = Disponivel)
        var query = _context.Motos
            .Include(m => m.Patio)
            .Where(m => m.Status == StatusMoto.Disponivel);

        if (motoIds != null && motoIds.Any())
        {
            query = query.Where(m => motoIds.Contains(m.MotoId));
        }

        var motos = await query.AsNoTracking().ToListAsync();

        // Buscar todos os pátios ou apenas os especificados
        var patiosQuery = _context.Patios.AsQueryable();

        if (patioIds != null && patioIds.Any())
        {
            patiosQuery = patiosQuery.Where(p => patioIds.Contains(p.PatioId));
        }

        var patios = await patiosQuery.AsNoTracking().ToListAsync();

        // Buscar contagem de motos por pátio separadamente para evitar ciclo
        var patioIdsList = patios.Select(p => p.PatioId).ToList();
        var quantidadeMotosPorPatio = await _context.Motos
            .Where(m => patioIdsList.Contains(m.PatioId) && m.Status == StatusMoto.Disponivel)
            .GroupBy(m => m.PatioId)
            .Select(g => new { PatioId = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.PatioId, x => x.Quantidade);

        if (!motos.Any() || !patios.Any())
        {
            return new List<RedistribuicaoOutput>();
        }

        // Calcular métricas globais usando o dicionário
        var totalMotos = quantidadeMotosPorPatio.Values.Sum();
        var mediaMotosPorPatio = totalMotos > 0 && patios.Any() ? (float)totalMotos / patios.Count : 0;

        var recomendacoes = new List<RedistribuicaoOutput>();

        // Para cada moto disponível, avaliar todos os pátios possíveis
        foreach (var moto in motos)
        {
            var patioAtual = patios.FirstOrDefault(p => p.PatioId == moto.PatioId);
            if (patioAtual == null) continue;

            // Usar dicionário ao invés de acessar propriedade de navegação para evitar ciclo
            var quantidadeMotosAtual = quantidadeMotosPorPatio.GetValueOrDefault(patioAtual.PatioId, 0);
            var taxaOcupacaoAtual = mediaMotosPorPatio > 0 ? quantidadeMotosAtual / mediaMotosPorPatio : 0;

            foreach (var patioDestino in patios)
            {
                if (patioDestino.PatioId == moto.PatioId) continue; // Não recomendar o mesmo pátio

                // Usar dicionário ao invés de acessar propriedade de navegação para evitar ciclo
                var quantidadeMotosDestino = quantidadeMotosPorPatio.GetValueOrDefault(patioDestino.PatioId, 0);
                var taxaOcupacaoDestino = mediaMotosPorPatio > 0 ? quantidadeMotosDestino / mediaMotosPorPatio : 0;

                // Calcular distância entre pátios
                var distanciaKm = CalcularDistancia(
                    (double)patioAtual.Latitude, (double)patioAtual.Longitude,
                    (double)patioDestino.Latitude, (double)patioDestino.Longitude
                );

                // Calcular diferença de equilíbrio (quanto mais próxima de 0, melhor)
                var diferencaEquilibrio = Math.Abs(taxaOcupacaoDestino + (1 / mediaMotosPorPatio) - taxaOcupacaoAtual);

                // Preparar input para predição
                var input = new RedistribuicaoInput
                {
                    PatioAtualId = ConvertToFloatId(patioAtual.PatioId),
                    PatioDestinoId = ConvertToFloatId(patioDestino.PatioId),
                    QuantidadeMotosAtual = quantidadeMotosAtual,
                    QuantidadeMotosDestino = quantidadeMotosDestino,
                    CategoriaPatioAtual = (float)patioAtual.Categoria,
                    CategoriaPatioDestino = (float)patioDestino.Categoria,
                    StatusMoto = (float)moto.Status,
                    DistanciaKm = (float)distanciaKm,
                    TaxaOcupacaoAtual = taxaOcupacaoAtual,
                    TaxaOcupacaoDestino = taxaOcupacaoDestino,
                    DiferencaEquilibrio = diferencaEquilibrio,
                    MediaGeralMotosPorPatio = mediaMotosPorPatio,
                    ScoreRedistribuicao = 0 // Será predito pelo modelo
                };

                // Fazer predição
                var predictionEngine = _mlContext!.Model.CreatePredictionEngine<RedistribuicaoInput, RedistribuicaoPrediction>(_trainedModel!);
                var prediction = predictionEngine.Predict(input);

                // Calcular impacto no equilíbrio (melhoria esperada)
                var impactoEquilibrio = diferencaEquilibrio - Math.Abs(taxaOcupacaoDestino - taxaOcupacaoAtual);

                // Gerar motivos da recomendação
                var motivos = GerarMotivos(patioAtual, patioDestino, quantidadeMotosAtual, quantidadeMotosDestino, mediaMotosPorPatio, distanciaKm);

                // Gerar recomendação descritiva
                var recomendacaoTexto = GerarRecomendacaoDescritiva(
                    moto, 
                    patioAtual, 
                    patioDestino, 
                    quantidadeMotosAtual, 
                    quantidadeMotosDestino, 
                    distanciaKm,
                    impactoEquilibrio,
                    prediction.Score
                );

                recomendacoes.Add(new RedistribuicaoOutput
                {
                    MotoId = moto.MotoId,
                    MotoModelo = moto.Modelo,
                    MotoPlaca = moto.Placa,
                    PatioOrigemId = patioAtual.PatioId,
                    PatioOrigemNome = patioAtual.Nome,
                    Score = prediction.Score,
                    PatioDestinoId = patioDestino.PatioId,
                    PatioDestinoNome = patioDestino.Nome,
                    Motivos = motivos,
                    ImpactoEquilibrio = impactoEquilibrio,
                    Recomendacao = recomendacaoTexto
                });
            }
        }

        // Agrupar por moto e selecionar apenas a melhor recomendação para cada moto
        // Isso evita múltiplas recomendações para a mesma moto
        var recomendacoesUnicas = recomendacoes
            .GroupBy(r => r.MotoId)
            .Select(g => g
                .OrderByDescending(r => r.Score)
                .ThenByDescending(r => r.ImpactoEquilibrio)
                .First())
            .OrderByDescending(r => r.Score)
            .ThenByDescending(r => r.ImpactoEquilibrio)
            .Take(50) // Limitar a 50 melhores recomendações
            .ToList();

        return recomendacoesUnicas;
    }

    /// <summary>
    /// Gera dados sintéticos de treinamento baseados na distribuição real
    /// </summary>
    private List<RedistribuicaoInput> GerarDadosTreinamento(
        List<Patio> patios, 
        List<Moto> motos,
        Dictionary<Guid, int> quantidadeMotosPorPatio)
    {
        var trainingData = new List<RedistribuicaoInput>();
        var totalMotos = quantidadeMotosPorPatio.Values.Sum();
        var mediaMotosPorPatio = patios.Any() ? (float)totalMotos / patios.Count : 0;

        // Gerar combinações de redistribuição
        foreach (var patioOrigem in patios)
        {
            // Usar dicionário ao invés de acessar propriedade de navegação
            var quantidadeMotosOrigem = quantidadeMotosPorPatio.GetValueOrDefault(patioOrigem.PatioId, 0);
            var taxaOcupacaoOrigem = mediaMotosPorPatio > 0 ? quantidadeMotosOrigem / mediaMotosPorPatio : 0;

            foreach (var patioDestino in patios)
            {
                if (patioOrigem.PatioId == patioDestino.PatioId) continue;

                // Usar dicionário ao invés de acessar propriedade de navegação
                var quantidadeMotosDestino = quantidadeMotosPorPatio.GetValueOrDefault(patioDestino.PatioId, 0);
                var taxaOcupacaoDestino = mediaMotosPorPatio > 0 ? quantidadeMotosDestino / mediaMotosPorPatio : 0;

                var distanciaKm = CalcularDistancia(
                    (double)patioOrigem.Latitude, (double)patioOrigem.Longitude,
                    (double)patioDestino.Latitude, (double)patioDestino.Longitude
                );

                var diferencaEquilibrio = Math.Abs(taxaOcupacaoDestino - taxaOcupacaoOrigem);

                // Calcular score ideal: quanto mais equilibrado após redistribuição, maior o score
                var scoreIdeal = CalcularScoreIdeal(
                    quantidadeMotosOrigem,
                    quantidadeMotosDestino,
                    mediaMotosPorPatio,
                    diferencaEquilibrio,
                    distanciaKm
                );

                // Adicionar variações para aumentar dataset de treinamento
                for (int i = 0; i < 3; i++) // 3 variações por combinação
                {
                    trainingData.Add(new RedistribuicaoInput
                    {
                        PatioAtualId = ConvertToFloatId(patioOrigem.PatioId),
                        PatioDestinoId = ConvertToFloatId(patioDestino.PatioId),
                        QuantidadeMotosAtual = quantidadeMotosOrigem + i,
                        QuantidadeMotosDestino = quantidadeMotosDestino - i,
                        CategoriaPatioAtual = (float)patioOrigem.Categoria,
                        CategoriaPatioDestino = (float)patioDestino.Categoria,
                        StatusMoto = (float)StatusMoto.Disponivel,
                        DistanciaKm = (float)distanciaKm,
                        TaxaOcupacaoAtual = taxaOcupacaoOrigem,
                        TaxaOcupacaoDestino = taxaOcupacaoDestino,
                        DiferencaEquilibrio = diferencaEquilibrio,
                        MediaGeralMotosPorPatio = mediaMotosPorPatio,
                        ScoreRedistribuicao = scoreIdeal
                    });
                }
            }
        }

        return trainingData;
    }

    /// <summary>
    /// Calcula score ideal baseado em critérios de equilíbrio
    /// </summary>
    private float CalcularScoreIdeal(float qtdAtual, float qtdDestino, float media, float diferencaEquilibrio, double distancia)
    {
        // Score base: quanto mais próximo da média, melhor
        var scoreEquilibrio = 1.0f - Math.Min(diferencaEquilibrio / 2.0f, 1.0f); // Normalizar entre 0-1

        // Penalizar distâncias muito grandes
        var scoreDistancia = distancia > 50 ? 0.3f : distancia > 20 ? 0.6f : 1.0f;

        // Score final é combinação dos fatores
        return (scoreEquilibrio * 0.7f + scoreDistancia * 0.3f);
    }

    /// <summary>
    /// Calcula distância em km entre duas coordenadas usando fórmula de Haversine
    /// </summary>
    private double CalcularDistancia(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Raio da Terra em km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double degrees) => degrees * Math.PI / 180;

    /// <summary>
    /// Converte Guid para float para uso no modelo ML
    /// </summary>
    private float ConvertToFloatId(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return BitConverter.ToSingle(bytes, 0);
    }

    /// <summary>
    /// Gera lista de motivos para a recomendação
    /// </summary>
    private List<string> GerarMotivos(
        Patio patioAtual,
        Patio patioDestino,
        int quantidadeAtual,
        int quantidadeDestino,
        float media,
        double distancia)
    {
        var motivos = new List<string>();

        if (quantidadeAtual > media * 1.2f)
        {
            motivos.Add($"Pátio origem está congestionado ({quantidadeAtual} motos)");
        }

        if (quantidadeDestino < media * 0.8f)
        {
            motivos.Add($"Pátio destino tem capacidade disponível ({quantidadeDestino} motos)");
        }

        if (distancia < 10)
        {
            motivos.Add("Distância curta entre pátios");
        }

        var diferenca = Math.Abs(quantidadeAtual - quantidadeDestino);
        if (diferenca > media * 0.3f)
        {
            motivos.Add("Melhora significativa no equilíbrio de distribuição");
        }

        if (patioDestino.Categoria == CategoriaPatio.Aluguel && patioAtual.Categoria != CategoriaPatio.Aluguel)
        {
            motivos.Add("Pátio destino é focado em aluguel");
        }

        return motivos;
    }

    /// <summary>
    /// Gera recomendação descritiva e acionável para o usuário
    /// </summary>
    private string GerarRecomendacaoDescritiva(
        Moto moto,
        Patio patioOrigem,
        Patio patioDestino,
        int quantidadeMotosOrigem,
        int quantidadeMotosDestino,
        double distanciaKm,
        float impactoEquilibrio,
        float score)
    {
        var placaInfo = string.IsNullOrWhiteSpace(moto.Placa) ? "sem placa" : $"placa {moto.Placa}";
        var distanciaTexto = distanciaKm < 5 ? "muito próxima" : distanciaKm < 20 ? "próxima" : "distante";
        
        // Determinar urgência e prioridade baseada no score
        string prioridade;
        string beneficio;
        
        if (score >= 0.7f)
        {
            prioridade = "ALTA PRIORIDADE";
            beneficio = "Esta redistribuição trará benefícios significativos";
        }
        else if (score >= 0.5f)
        {
            prioridade = "MÉDIA PRIORIDADE";
            beneficio = "Esta redistribuição melhorará o equilíbrio";
        }
        else
        {
            prioridade = "BAIXA PRIORIDADE";
            beneficio = "Esta redistribuição pode ser considerada";
        }

        // Descrever a situação atual e o que será alcançado
        var situacaoOrigem = quantidadeMotosOrigem == 0 
            ? "vazio" 
            : quantidadeMotosOrigem == 1 
                ? "com 1 moto" 
                : $"congestionado com {quantidadeMotosOrigem} motos";
        
        var situacaoDestino = quantidadeMotosDestino == 0 
            ? "vazio e com capacidade total" 
            : quantidadeMotosDestino == 1 
                ? "com apenas 1 moto e com boa capacidade" 
                : $"com {quantidadeMotosDestino} motos e com capacidade disponível";

        // Construir recomendação completa
        var recomendacao = $"[{prioridade}] " +
            $"Mover a moto {moto.Modelo} ({placaInfo}) " +
            $"do pátio '{patioOrigem.Nome}' ({situacaoOrigem}) " +
            $"para o pátio '{patioDestino.Nome}' ({situacaoDestino}). " +
            $"A distância entre os pátios é {distanciaTexto} ({distanciaKm:F1} km). " +
            $"{beneficio} no equilíbrio da distribuição de motos. " +
            $"Após esta movimentação, o pátio de origem ficará com {quantidadeMotosOrigem - 1} moto(s) " +
            $"e o pátio de destino ficará com {quantidadeMotosDestino + 1} moto(s).";

        return recomendacao;
    }
}


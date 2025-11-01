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
<<<<<<< Updated upstream
=======
<<<<<<< HEAD
            nameof(RedistribuicaoInput.CapacidadePatioAtual),
            nameof(RedistribuicaoInput.CapacidadePatioDestino),
            nameof(RedistribuicaoInput.PercentualOcupacaoAtual),
            nameof(RedistribuicaoInput.PercentualOcupacaoDestino),
            nameof(RedistribuicaoInput.EspacoDisponivelDestino),
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
<<<<<<< HEAD
        // IMPORTANTE: Se patioIds foi fornecido, também precisamos incluir os pátios onde as motos estão atualmente
        // para poder calcular corretamente a origem (patioAtual)
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
        var patiosQuery = _context.Patios.AsQueryable();

        if (patioIds != null && patioIds.Any())
        {
<<<<<<< Updated upstream
            patiosQuery = patiosQuery.Where(p => patioIds.Contains(p.PatioId));
=======
<<<<<<< HEAD
            // Obter IDs únicos dos pátios onde as motos solicitadas estão
            var patiosOrigemIds = motos.Select(m => m.PatioId).Distinct().ToList();
            
            // Combinar pátios solicitados + pátios de origem das motos
            var todosPatioIds = patioIds.Union(patiosOrigemIds).Distinct().ToList();
            
            patiosQuery = patiosQuery.Where(p => todosPatioIds.Contains(p.PatioId));
=======
            patiosQuery = patiosQuery.Where(p => patioIds.Contains(p.PatioId));
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
            var taxaOcupacaoAtual = mediaMotosPorPatio > 0 ? quantidadeMotosAtual / mediaMotosPorPatio : 0;
=======
<<<<<<< HEAD
            var capacidadeAtual = patioAtual.Capacidade;
            var percentualOcupacaoAtual = capacidadeAtual > 0 ? (float)quantidadeMotosAtual / capacidadeAtual : 0f;
=======
            var taxaOcupacaoAtual = mediaMotosPorPatio > 0 ? quantidadeMotosAtual / mediaMotosPorPatio : 0;
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes

            foreach (var patioDestino in patios)
            {
                if (patioDestino.PatioId == moto.PatioId) continue; // Não recomendar o mesmo pátio

<<<<<<< Updated upstream
                // Usar dicionário ao invés de acessar propriedade de navegação para evitar ciclo
                var quantidadeMotosDestino = quantidadeMotosPorPatio.GetValueOrDefault(patioDestino.PatioId, 0);
                var taxaOcupacaoDestino = mediaMotosPorPatio > 0 ? quantidadeMotosDestino / mediaMotosPorPatio : 0;
=======
<<<<<<< HEAD
                // Verificar se pátio destino tem espaço disponível (não recomendar se já está lotado)
                var quantidadeMotosDestino = quantidadeMotosPorPatio.GetValueOrDefault(patioDestino.PatioId, 0);
                var capacidadeDestino = patioDestino.Capacidade;
                var percentualOcupacaoDestino = capacidadeDestino > 0 ? (float)quantidadeMotosDestino / capacidadeDestino : 0f;
                
                // Não recomendar se pátio destino está lotado (>= 100% da capacidade)
                if (quantidadeMotosDestino >= capacidadeDestino) continue;
=======
                // Usar dicionário ao invés de acessar propriedade de navegação para evitar ciclo
                var quantidadeMotosDestino = quantidadeMotosPorPatio.GetValueOrDefault(patioDestino.PatioId, 0);
                var taxaOcupacaoDestino = mediaMotosPorPatio > 0 ? quantidadeMotosDestino / mediaMotosPorPatio : 0;
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes

                // Calcular distância entre pátios
                var distanciaKm = CalcularDistancia(
                    (double)patioAtual.Latitude, (double)patioAtual.Longitude,
                    (double)patioDestino.Latitude, (double)patioDestino.Longitude
                );

<<<<<<< Updated upstream
                // Calcular diferença de equilíbrio (quanto mais próxima de 0, melhor)
                var diferencaEquilibrio = Math.Abs(taxaOcupacaoDestino + (1 / mediaMotosPorPatio) - taxaOcupacaoAtual);
=======
<<<<<<< HEAD
                // Calcular espaço disponível no pátio destino
                var espacoDisponivelDestino = capacidadeDestino - quantidadeMotosDestino;
                
                // Calcular diferença de equilíbrio baseado em percentual de ocupação
                var diferencaEquilibrio = Math.Abs(percentualOcupacaoDestino - percentualOcupacaoAtual);
=======
                // Calcular diferença de equilíbrio (quanto mais próxima de 0, melhor)
                var diferencaEquilibrio = Math.Abs(taxaOcupacaoDestino + (1 / mediaMotosPorPatio) - taxaOcupacaoAtual);
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes

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
<<<<<<< Updated upstream
                    TaxaOcupacaoAtual = taxaOcupacaoAtual,
                    TaxaOcupacaoDestino = taxaOcupacaoDestino,
=======
<<<<<<< HEAD
                    TaxaOcupacaoAtual = percentualOcupacaoAtual, // Agora usando percentual baseado em capacidade
                    TaxaOcupacaoDestino = percentualOcupacaoDestino,
                    CapacidadePatioAtual = capacidadeAtual,
                    CapacidadePatioDestino = capacidadeDestino,
                    PercentualOcupacaoAtual = percentualOcupacaoAtual,
                    PercentualOcupacaoDestino = percentualOcupacaoDestino,
                    EspacoDisponivelDestino = espacoDisponivelDestino,
=======
                    TaxaOcupacaoAtual = taxaOcupacaoAtual,
                    TaxaOcupacaoDestino = taxaOcupacaoDestino,
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
                    DiferencaEquilibrio = diferencaEquilibrio,
                    MediaGeralMotosPorPatio = mediaMotosPorPatio,
                    ScoreRedistribuicao = 0 // Será predito pelo modelo
                };

                // Fazer predição
                var predictionEngine = _mlContext!.Model.CreatePredictionEngine<RedistribuicaoInput, RedistribuicaoPrediction>(_trainedModel!);
                var prediction = predictionEngine.Predict(input);

<<<<<<< Updated upstream
=======
<<<<<<< HEAD
                // Calcular impacto no equilíbrio baseado em percentual de ocupação
                var percentualOcupacaoAtualAposMovimento = capacidadeAtual > 0 ? (float)(quantidadeMotosAtual - 1) / capacidadeAtual : 0f;
                var percentualOcupacaoDestinoAposMovimento = capacidadeDestino > 0 ? (float)(quantidadeMotosDestino + 1) / capacidadeDestino : 0f;
                var impactoEquilibrio = diferencaEquilibrio - Math.Abs(percentualOcupacaoDestinoAposMovimento - percentualOcupacaoAtualAposMovimento);

                // Gerar motivos da recomendação usando capacidade real
                var motivos = GerarMotivos(
                    patioAtual, 
                    patioDestino, 
                    quantidadeMotosAtual, 
                    quantidadeMotosDestino, 
                    capacidadeAtual,
                    capacidadeDestino,
                    percentualOcupacaoAtual,
                    percentualOcupacaoDestino,
                    distanciaKm
                );

                // Gerar recomendação descritiva usando capacidade real
=======
>>>>>>> Stashed changes
                // Calcular impacto no equilíbrio (melhoria esperada)
                var impactoEquilibrio = diferencaEquilibrio - Math.Abs(taxaOcupacaoDestino - taxaOcupacaoAtual);

                // Gerar motivos da recomendação
                var motivos = GerarMotivos(patioAtual, patioDestino, quantidadeMotosAtual, quantidadeMotosDestino, mediaMotosPorPatio, distanciaKm);

                // Gerar recomendação descritiva
<<<<<<< Updated upstream
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
                var recomendacaoTexto = GerarRecomendacaoDescritiva(
                    moto, 
                    patioAtual, 
                    patioDestino, 
                    quantidadeMotosAtual, 
<<<<<<< Updated upstream
                    quantidadeMotosDestino, 
=======
<<<<<<< HEAD
                    quantidadeMotosDestino,
                    capacidadeAtual,
                    capacidadeDestino,
                    percentualOcupacaoAtual,
                    percentualOcupacaoDestino,
=======
                    quantidadeMotosDestino, 
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
        // Gerar combinações de redistribuição
=======
<<<<<<< HEAD
        // Gerar combinações de redistribuição baseadas em capacidade real
=======
        // Gerar combinações de redistribuição
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
        foreach (var patioOrigem in patios)
        {
            // Usar dicionário ao invés de acessar propriedade de navegação
            var quantidadeMotosOrigem = quantidadeMotosPorPatio.GetValueOrDefault(patioOrigem.PatioId, 0);
<<<<<<< Updated upstream
            var taxaOcupacaoOrigem = mediaMotosPorPatio > 0 ? quantidadeMotosOrigem / mediaMotosPorPatio : 0;
=======
<<<<<<< HEAD
            var capacidadeOrigem = patioOrigem.Capacidade;
            var percentualOcupacaoOrigem = capacidadeOrigem > 0 ? (float)quantidadeMotosOrigem / capacidadeOrigem : 0f;
=======
            var taxaOcupacaoOrigem = mediaMotosPorPatio > 0 ? quantidadeMotosOrigem / mediaMotosPorPatio : 0;
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes

            foreach (var patioDestino in patios)
            {
                if (patioOrigem.PatioId == patioDestino.PatioId) continue;

<<<<<<< Updated upstream
                // Usar dicionário ao invés de acessar propriedade de navegação
                var quantidadeMotosDestino = quantidadeMotosPorPatio.GetValueOrDefault(patioDestino.PatioId, 0);
                var taxaOcupacaoDestino = mediaMotosPorPatio > 0 ? quantidadeMotosDestino / mediaMotosPorPatio : 0;
=======
<<<<<<< HEAD
                // Não treinar com pátios lotados como destino
                var quantidadeMotosDestino = quantidadeMotosPorPatio.GetValueOrDefault(patioDestino.PatioId, 0);
                var capacidadeDestino = patioDestino.Capacidade;
                if (quantidadeMotosDestino >= capacidadeDestino) continue;

                var percentualOcupacaoDestino = capacidadeDestino > 0 ? (float)quantidadeMotosDestino / capacidadeDestino : 0f;
                var espacoDisponivelDestino = capacidadeDestino - quantidadeMotosDestino;
=======
                // Usar dicionário ao invés de acessar propriedade de navegação
                var quantidadeMotosDestino = quantidadeMotosPorPatio.GetValueOrDefault(patioDestino.PatioId, 0);
                var taxaOcupacaoDestino = mediaMotosPorPatio > 0 ? quantidadeMotosDestino / mediaMotosPorPatio : 0;
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes

                var distanciaKm = CalcularDistancia(
                    (double)patioOrigem.Latitude, (double)patioOrigem.Longitude,
                    (double)patioDestino.Latitude, (double)patioDestino.Longitude
                );

<<<<<<< Updated upstream
=======
<<<<<<< HEAD
                var diferencaEquilibrio = Math.Abs(percentualOcupacaoDestino - percentualOcupacaoOrigem);

                // Calcular score ideal baseado em capacidade real
                var scoreIdeal = CalcularScoreIdeal(
                    quantidadeMotosOrigem,
                    quantidadeMotosDestino,
                    capacidadeOrigem,
                    capacidadeDestino,
                    percentualOcupacaoOrigem,
                    percentualOcupacaoDestino,
=======
>>>>>>> Stashed changes
                var diferencaEquilibrio = Math.Abs(taxaOcupacaoDestino - taxaOcupacaoOrigem);

                // Calcular score ideal: quanto mais equilibrado após redistribuição, maior o score
                var scoreIdeal = CalcularScoreIdeal(
                    quantidadeMotosOrigem,
                    quantidadeMotosDestino,
                    mediaMotosPorPatio,
<<<<<<< Updated upstream
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
                    diferencaEquilibrio,
                    distanciaKm
                );

                // Adicionar variações para aumentar dataset de treinamento
                for (int i = 0; i < 3; i++) // 3 variações por combinação
                {
<<<<<<< Updated upstream
=======
<<<<<<< HEAD
                    var qtdOrigemVar = Math.Max(0, quantidadeMotosOrigem + (i - 1));
                    var qtdDestinoVar = Math.Min(capacidadeDestino - 1, quantidadeMotosDestino - (i - 1));
                    var percentualOrigemVar = capacidadeOrigem > 0 ? (float)qtdOrigemVar / capacidadeOrigem : 0f;
                    var percentualDestinoVar = capacidadeDestino > 0 ? (float)qtdDestinoVar / capacidadeDestino : 0f;

=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
                    trainingData.Add(new RedistribuicaoInput
                    {
                        PatioAtualId = ConvertToFloatId(patioOrigem.PatioId),
                        PatioDestinoId = ConvertToFloatId(patioDestino.PatioId),
<<<<<<< Updated upstream
                        QuantidadeMotosAtual = quantidadeMotosOrigem + i,
                        QuantidadeMotosDestino = quantidadeMotosDestino - i,
=======
<<<<<<< HEAD
                        QuantidadeMotosAtual = qtdOrigemVar,
                        QuantidadeMotosDestino = qtdDestinoVar,
=======
                        QuantidadeMotosAtual = quantidadeMotosOrigem + i,
                        QuantidadeMotosDestino = quantidadeMotosDestino - i,
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
                        CategoriaPatioAtual = (float)patioOrigem.Categoria,
                        CategoriaPatioDestino = (float)patioDestino.Categoria,
                        StatusMoto = (float)StatusMoto.Disponivel,
                        DistanciaKm = (float)distanciaKm,
<<<<<<< Updated upstream
                        TaxaOcupacaoAtual = taxaOcupacaoOrigem,
                        TaxaOcupacaoDestino = taxaOcupacaoDestino,
                        DiferencaEquilibrio = diferencaEquilibrio,
=======
<<<<<<< HEAD
                        TaxaOcupacaoAtual = percentualOrigemVar,
                        TaxaOcupacaoDestino = percentualDestinoVar,
                        CapacidadePatioAtual = capacidadeOrigem,
                        CapacidadePatioDestino = capacidadeDestino,
                        PercentualOcupacaoAtual = percentualOrigemVar,
                        PercentualOcupacaoDestino = percentualDestinoVar,
                        EspacoDisponivelDestino = capacidadeDestino - qtdDestinoVar,
                        DiferencaEquilibrio = Math.Abs(percentualDestinoVar - percentualOrigemVar),
=======
                        TaxaOcupacaoAtual = taxaOcupacaoOrigem,
                        TaxaOcupacaoDestino = taxaOcupacaoDestino,
                        DiferencaEquilibrio = diferencaEquilibrio,
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
                        MediaGeralMotosPorPatio = mediaMotosPorPatio,
                        ScoreRedistribuicao = scoreIdeal
                    });
                }
            }
        }

        return trainingData;
    }

    /// <summary>
<<<<<<< Updated upstream
=======
<<<<<<< HEAD
    /// Calcula score ideal baseado em critérios de equilíbrio usando capacidade real
    /// </summary>
    private float CalcularScoreIdeal(
        int qtdAtual, 
        int qtdDestino, 
        int capacidadeAtual,
        int capacidadeDestino,
        float percentualOcupacaoAtual,
        float percentualOcupacaoDestino,
        float diferencaEquilibrio, 
        double distancia)
    {
        // Score baseado em melhoria de ocupação percentual
        // Quanto maior a diferença de ocupação (origem alta -> destino baixa), melhor
        var melhoriaOcupacao = percentualOcupacaoAtual - percentualOcupacaoDestino;
        var scoreEquilibrio = Math.Max(0, Math.Min(1, melhoriaOcupacao + 0.5f)); // Normalizar entre 0-1

        // Bonus para pátios destino com muito espaço disponível
        var espacoDisponivel = capacidadeDestino - qtdDestino;
        var scoreEspacoDisponivel = capacidadeDestino > 0 
            ? Math.Min(1.0f, (float)espacoDisponivel / capacidadeDestino * 2f) 
            : 0f;

        // Penalizar se origem não está realmente congestionada (< 60% ocupação)
        var scoreNecessidade = percentualOcupacaoAtual > 0.8f ? 1.0f 
            : percentualOcupacaoAtual > 0.6f ? 0.7f 
            : percentualOcupacaoAtual > 0.4f ? 0.4f 
            : 0.2f;
=======
>>>>>>> Stashed changes
    /// Calcula score ideal baseado em critérios de equilíbrio
    /// </summary>
    private float CalcularScoreIdeal(float qtdAtual, float qtdDestino, float media, float diferencaEquilibrio, double distancia)
    {
        // Score base: quanto mais próximo da média, melhor
        var scoreEquilibrio = 1.0f - Math.Min(diferencaEquilibrio / 2.0f, 1.0f); // Normalizar entre 0-1
<<<<<<< Updated upstream
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes

        // Penalizar distâncias muito grandes
        var scoreDistancia = distancia > 50 ? 0.3f : distancia > 20 ? 0.6f : 1.0f;

<<<<<<< Updated upstream
        // Score final é combinação dos fatores
        return (scoreEquilibrio * 0.7f + scoreDistancia * 0.3f);
=======
<<<<<<< HEAD
        // Score final é combinação ponderada dos fatores
        return (scoreEquilibrio * 0.4f + scoreEspacoDisponivel * 0.2f + scoreNecessidade * 0.2f + scoreDistancia * 0.2f);
=======
        // Score final é combinação dos fatores
        return (scoreEquilibrio * 0.7f + scoreDistancia * 0.3f);
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
    /// Gera lista de motivos para a recomendação
=======
<<<<<<< HEAD
    /// Gera lista de motivos para a recomendação baseada em capacidade real
=======
    /// Gera lista de motivos para a recomendação
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
    /// </summary>
    private List<string> GerarMotivos(
        Patio patioAtual,
        Patio patioDestino,
        int quantidadeAtual,
        int quantidadeDestino,
<<<<<<< Updated upstream
        float media,
=======
<<<<<<< HEAD
        int capacidadeAtual,
        int capacidadeDestino,
        float percentualOcupacaoAtual,
        float percentualOcupacaoDestino,
=======
        float media,
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
        double distancia)
    {
        var motivos = new List<string>();

<<<<<<< Updated upstream
=======
<<<<<<< HEAD
        // Pátio origem congestionado (> 80% da capacidade)
        if (percentualOcupacaoAtual > 0.8f)
        {
            motivos.Add($"Pátio origem está congestionado ({quantidadeAtual}/{capacidadeAtual} motos - {percentualOcupacaoAtual:P0} da capacidade)");
        }
        else if (percentualOcupacaoAtual > 0.6f)
        {
            motivos.Add($"Pátio origem está com alta ocupação ({quantidadeAtual}/{capacidadeAtual} motos - {percentualOcupacaoAtual:P0} da capacidade)");
        }

        // Pátio destino com espaço disponível
        if (percentualOcupacaoDestino < 0.5f)
        {
            motivos.Add($"Pátio destino tem boa capacidade disponível ({quantidadeDestino}/{capacidadeDestino} motos - {percentualOcupacaoDestino:P0} da capacidade)");
        }
        else if (percentualOcupacaoDestino < 0.7f)
        {
            motivos.Add($"Pátio destino tem capacidade disponível ({quantidadeDestino}/{capacidadeDestino} motos - {percentualOcupacaoDestino:P0} da capacidade)");
        }

        var espacoDisponivel = capacidadeDestino - quantidadeDestino;
        if (espacoDisponivel > 5)
        {
            motivos.Add($"Pátio destino tem {espacoDisponivel} vagas disponíveis");
=======
>>>>>>> Stashed changes
        if (quantidadeAtual > media * 1.2f)
        {
            motivos.Add($"Pátio origem está congestionado ({quantidadeAtual} motos)");
        }

        if (quantidadeDestino < media * 0.8f)
        {
            motivos.Add($"Pátio destino tem capacidade disponível ({quantidadeDestino} motos)");
<<<<<<< Updated upstream
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
        }

        if (distancia < 10)
        {
<<<<<<< Updated upstream
=======
<<<<<<< HEAD
            motivos.Add($"Distância curta entre pátios ({distancia:F1} km)");
        }

        // Melhora no equilíbrio de ocupação
        var melhoriaOcupacao = percentualOcupacaoAtual - percentualOcupacaoDestino;
        if (melhoriaOcupacao > 0.2f)
=======
>>>>>>> Stashed changes
            motivos.Add("Distância curta entre pátios");
        }

        var diferenca = Math.Abs(quantidadeAtual - quantidadeDestino);
        if (diferenca > media * 0.3f)
<<<<<<< Updated upstream
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
    /// Gera recomendação descritiva e acionável para o usuário
=======
<<<<<<< HEAD
    /// Gera recomendação descritiva e acionável para o usuário usando capacidade real
=======
    /// Gera recomendação descritiva e acionável para o usuário
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
    /// </summary>
    private string GerarRecomendacaoDescritiva(
        Moto moto,
        Patio patioOrigem,
        Patio patioDestino,
        int quantidadeMotosOrigem,
        int quantidadeMotosDestino,
<<<<<<< Updated upstream
=======
<<<<<<< HEAD
        int capacidadeOrigem,
        int capacidadeDestino,
        float percentualOcupacaoOrigem,
        float percentualOcupacaoDestino,
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
=======
<<<<<<< HEAD
        // Descrever a situação atual baseada em capacidade e ocupação percentual
        var situacaoOrigem = percentualOcupacaoOrigem >= 0.9f
            ? $"lotado ({quantidadeMotosOrigem}/{capacidadeOrigem} motos - {percentualOcupacaoOrigem:P0})"
            : percentualOcupacaoOrigem >= 0.7f
                ? $"alto ({quantidadeMotosOrigem}/{capacidadeOrigem} motos - {percentualOcupacaoOrigem:P0})"
                : quantidadeMotosOrigem == 0
                    ? "vazio"
                    : $"com ocupação moderada ({quantidadeMotosOrigem}/{capacidadeOrigem} motos - {percentualOcupacaoOrigem:P0})";
        
        var espacoDisponivel = capacidadeDestino - quantidadeMotosDestino;
        var situacaoDestino = percentualOcupacaoDestino < 0.3f
            ? $"com muito espaço disponível ({quantidadeMotosDestino}/{capacidadeDestino} motos - {percentualOcupacaoDestino:P0}, {espacoDisponivel} vagas)"
            : percentualOcupacaoDestino < 0.6f
                ? $"com espaço disponível ({quantidadeMotosDestino}/{capacidadeDestino} motos - {percentualOcupacaoDestino:P0}, {espacoDisponivel} vagas)"
                : $"com algum espaço ({quantidadeMotosDestino}/{capacidadeDestino} motos - {percentualOcupacaoDestino:P0}, {espacoDisponivel} vagas)";

        var ocupacaoOrigemApos = capacidadeOrigem > 0 ? (float)(quantidadeMotosOrigem - 1) / capacidadeOrigem : 0f;
        var ocupacaoDestinoApos = capacidadeDestino > 0 ? (float)(quantidadeMotosDestino + 1) / capacidadeDestino : 0f;
=======
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes

        // Construir recomendação completa
        var recomendacao = $"[{prioridade}] " +
            $"Mover a moto {moto.Modelo} ({placaInfo}) " +
<<<<<<< Updated upstream
=======
<<<<<<< HEAD
            $"do pátio '{patioOrigem.Nome}' (atualmente {situacaoOrigem}) " +
            $"para o pátio '{patioDestino.Nome}' (atualmente {situacaoDestino}). " +
            $"A distância entre os pátios é {distanciaTexto} ({distanciaKm:F1} km). " +
            $"{beneficio} no equilíbrio da distribuição de motos. " +
            $"Após esta movimentação, o pátio de origem ficará com {quantidadeMotosOrigem - 1}/{capacidadeOrigem} motos ({ocupacaoOrigemApos:P0}) " +
            $"e o pátio de destino ficará com {quantidadeMotosDestino + 1}/{capacidadeDestino} motos ({ocupacaoDestinoApos:P0}).";
=======
>>>>>>> Stashed changes
            $"do pátio '{patioOrigem.Nome}' ({situacaoOrigem}) " +
            $"para o pátio '{patioDestino.Nome}' ({situacaoDestino}). " +
            $"A distância entre os pátios é {distanciaTexto} ({distanciaKm:F1} km). " +
            $"{beneficio} no equilíbrio da distribuição de motos. " +
            $"Após esta movimentação, o pátio de origem ficará com {quantidadeMotosOrigem - 1} moto(s) " +
            $"e o pátio de destino ficará com {quantidadeMotosDestino + 1} moto(s).";
<<<<<<< Updated upstream
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes

        return recomendacao;
    }
}


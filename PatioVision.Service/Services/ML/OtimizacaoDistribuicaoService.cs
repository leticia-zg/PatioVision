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
/// Serviço de Machine Learning para otimização de distribuição de motos entre pátios.
/// </summary>
public class OtimizacaoDistribuicaoService
{
    private readonly AppDbContext _context;
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private readonly string _modelPath;

    public OtimizacaoDistribuicaoService(AppDbContext context)
    {
        _context = context;
        _mlContext = new MLContext(seed: 0);
        
        // Define o caminho do modelo - cria na pasta atual da aplicação ou usa AppDomain
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory ?? AppContext.BaseDirectory;
        var mlModelsPath = Path.Combine(baseDirectory, "MLModels");
        _modelPath = Path.Combine(mlModelsPath, "OtimizacaoDistribuicao.zip");
        
        LoadOrCreateModel();
    }

    /// <summary>
    /// Carrega o modelo salvo ou cria um novo modelo com dados sintéticos para demonstração.
    /// </summary>
    private void LoadOrCreateModel()
    {
        if (File.Exists(_modelPath))
        {
            try
            {
                _model = _mlContext.Model.Load(_modelPath, out var schema);
                return;
            }
            catch
            {
                // Se falhar ao carregar, cria um novo modelo
            }
        }

        // Cria modelo com dados sintéticos baseado em heurísticas
        CreateModelWithHeuristics();
    }

    /// <summary>
    /// Cria um modelo inicial baseado em heurísticas do domínio.
    /// Em produção, isso seria substituído por treinamento com dados reais.
    /// </summary>
    private void CreateModelWithHeuristics()
    {
        var trainingData = GenerateSyntheticTrainingData();
        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        // Converte colunas int para float antes da concatenação
        var pipeline = _mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(OtimizacaoDistribuicaoInput.ScoreAdequacao))
            .Append(_mlContext.Transforms.Categorical.OneHotEncoding(
                outputColumnName: "ModeloMotoEncoded",
                inputColumnName: nameof(OtimizacaoDistribuicaoInput.ModeloMoto)))
            .Append(_mlContext.Transforms.Conversion.ConvertType(
                outputColumnName: "StatusMotoEncodedFloat",
                inputColumnName: nameof(OtimizacaoDistribuicaoInput.StatusMotoEncoded),
                outputKind: Microsoft.ML.Data.DataKind.Single))
            .Append(_mlContext.Transforms.Conversion.ConvertType(
                outputColumnName: "CategoriaPatioEncodedFloat",
                inputColumnName: nameof(OtimizacaoDistribuicaoInput.CategoriaPatioEncoded),
                outputKind: Microsoft.ML.Data.DataKind.Single))
            .Append(_mlContext.Transforms.Conversion.ConvertType(
                outputColumnName: "DiaDaSemanaFloat",
                inputColumnName: nameof(OtimizacaoDistribuicaoInput.DiaDaSemana),
                outputKind: Microsoft.ML.Data.DataKind.Single))
            .Append(_mlContext.Transforms.Conversion.ConvertType(
                outputColumnName: "MesDoAnoFloat",
                inputColumnName: nameof(OtimizacaoDistribuicaoInput.MesDoAno),
                outputKind: Microsoft.ML.Data.DataKind.Single))
            .Append(_mlContext.Transforms.Conversion.ConvertType(
                outputColumnName: "HoraDoDiaFloat",
                inputColumnName: nameof(OtimizacaoDistribuicaoInput.HoraDoDia),
                outputKind: Microsoft.ML.Data.DataKind.Single))
            .Append(_mlContext.Transforms.Concatenate(
                "Features",
                "ModeloMotoEncoded",
                "StatusMotoEncodedFloat",
                nameof(OtimizacaoDistribuicaoInput.DiasDesdeCadastro),
                nameof(OtimizacaoDistribuicaoInput.TempoMedioDisponivel),
                nameof(OtimizacaoDistribuicaoInput.TempoMedioAlugada),
                nameof(OtimizacaoDistribuicaoInput.TempoMedioManutencao),
                "CategoriaPatioEncodedFloat",
                nameof(OtimizacaoDistribuicaoInput.Latitude),
                nameof(OtimizacaoDistribuicaoInput.Longitude),
                nameof(OtimizacaoDistribuicaoInput.CapacidadePatio),
                nameof(OtimizacaoDistribuicaoInput.MotosAtuais),
                nameof(OtimizacaoDistribuicaoInput.TaxaOcupacao),
                nameof(OtimizacaoDistribuicaoInput.TaxaAluguelHistorica),
                "DiaDaSemanaFloat",
                "MesDoAnoFloat",
                "HoraDoDiaFloat"))
            .Append(_mlContext.Regression.Trainers.FastTree(
                labelColumnName: "Label",
                featureColumnName: "Features",
                numberOfLeaves: 20,
                numberOfTrees: 100,
                minimumExampleCountPerLeaf: 10));

        _model = pipeline.Fit(dataView);

        // Salva o modelo
        var directory = Path.GetDirectoryName(_modelPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
            _mlContext.Model.Save(_model, dataView.Schema, _modelPath);
        }
    }

    /// <summary>
    /// Gera dados sintéticos de treinamento baseados em regras de negócio.
    /// </summary>
    private List<OtimizacaoDistribuicaoInput> GenerateSyntheticTrainingData()
    {
        var data = new List<OtimizacaoDistribuicaoInput>();
        var random = new Random(42); // Seed fixo para reprodutibilidade

        // Simula diferentes cenários
        for (int i = 0; i < 1000; i++)
        {
            var status = random.Next(0, 3);
            var categoria = random.Next(0, 3);
            var capacidade = random.Next(20, 101);
            var motosAtuais = random.Next(5, capacidade);
            var taxaOcupacao = (float)motosAtuais / capacidade;
            var taxaAluguel = (float)random.NextDouble() * 0.8f; // 0-80%

            // Calcula score baseado em heurísticas
            float score = CalcularScoreHeuristico(
                status, categoria, taxaOcupacao, taxaAluguel, capacidade, motosAtuais);

            data.Add(new OtimizacaoDistribuicaoInput
            {
                ModeloMoto = $"Modelo{random.Next(1, 6)}",
                StatusMotoEncoded = status,
                DiasDesdeCadastro = random.Next(1, 365),
                TempoMedioDisponivel = random.Next(1, 100),
                TempoMedioAlugada = random.Next(1, 50),
                TempoMedioManutencao = random.Next(1, 30),
                CategoriaPatioEncoded = categoria,
                Latitude = (float)(random.NextDouble() * 180 - 90),
                Longitude = (float)(random.NextDouble() * 360 - 180),
                CapacidadePatio = capacidade,
                MotosAtuais = motosAtuais,
                TaxaOcupacao = taxaOcupacao,
                TaxaAluguelHistorica = taxaAluguel,
                DiaDaSemana = random.Next(0, 7),
                MesDoAno = random.Next(1, 13),
                HoraDoDia = random.Next(0, 24),
                ScoreAdequacao = score
            });
        }

        return data;
    }

    /// <summary>
    /// Calcula score de adequação usando heurísticas do domínio.
    /// </summary>
    private float CalcularScoreHeuristico(
        int status, int categoria, float taxaOcupacao, float taxaAluguel,
        float capacidade, float motosAtuais)
    {
        float score = 0.5f; // Base

        // Status Disponivel funciona melhor em pátios de Aluguel
        if (status == 0 && categoria == 2) score += 0.2f;
        // Status EmManutencao funciona melhor em pátios de Manutencao
        if (status == 1 && categoria == 1) score += 0.3f;

        // Taxa de ocupação ideal: 60-80%
        if (taxaOcupacao >= 0.6f && taxaOcupacao <= 0.8f) score += 0.15f;
        else if (taxaOcupacao > 0.9f) score -= 0.2f; // Pátio muito cheio
        else if (taxaOcupacao < 0.3f) score -= 0.1f; // Pátio muito vazio

        // Taxa de aluguel alta indica demanda, score positivo
        if (taxaAluguel > 0.5f) score += 0.1f;

        // Capacidade maior oferece mais flexibilidade
        if (capacidade > 50) score += 0.05f;

        return Math.Clamp(score, 0f, 1f);
    }

    /// <summary>
    /// Prediz o score de adequação de uma moto em um pátio.
    /// </summary>
    public float PredictScore(OtimizacaoDistribuicaoInput input)
    {
        if (_model == null)
        {
            LoadOrCreateModel();
        }

        var predictionEngine = _mlContext.Model.CreatePredictionEngine<OtimizacaoDistribuicaoInput, OtimizacaoDistribuicaoOutput>(_model!);
        var prediction = predictionEngine.Predict(input);

        return Math.Clamp(prediction.ScoreAdequacao, 0f, 1f);
    }

    /// <summary>
    /// Analisa todas as motos e pátios, gerando recomendações de redistribuição.
    /// </summary>
    public async Task<(List<(Moto moto, Patio patioAtual, Patio patioRecomendado, float score)> recomendacoes, int totalAnalisadas)> 
        GerarRecomendacoesAsync(List<Guid>? motoIds = null, bool apenasDisponiveis = true, bool incluirTodasMotos = false)
    {
        var motos = await _context.Motos
            .Include(m => m.Patio)
            .Include(m => m.Dispositivo)
            .AsQueryable()
            .Where(m => motoIds == null || motoIds.Contains(m.MotoId))
            .Where(m => !apenasDisponiveis || m.Status == StatusMoto.Disponivel)
            .ToListAsync();

        var patios = await _context.Patios
            .Include(p => p.Motos)
            .ToListAsync();

        // Conta quantas motos foram encontradas (para diagnóstico)
        int totalMotosAnalisadas = motos.Count;

        var recomendacoes = new List<(Moto moto, Patio patioAtual, Patio patioRecomendado, float score)>();

        foreach (var moto in motos)
        {
            if (moto.Patio == null) continue;

            var patiosDisponiveis = patios.Where(p => p.PatioId != moto.PatioId).ToList();
            
            // Se não há outros pátios disponíveis, pula esta moto
            if (!patiosDisponiveis.Any()) continue;

            float melhorScore = 0f;
            Patio? melhorPatio = null;
            float scoreAtual = 0f;

            // Calcula score atual
            var inputAtual = CriarInput(moto, moto.Patio, patios);
            scoreAtual = PredictScore(inputAtual);

            // Testa todos os outros pátios
            foreach (var patio in patiosDisponiveis)
            {
                var input = CriarInput(moto, patio, patios);
                var score = PredictScore(input);

                // Se incluirTodasMotos for true, aceita qualquer melhoria
                // Caso contrário, exige melhoria significativa (0.1f)
                bool aceitaRecomendacao = incluirTodasMotos 
                    ? score > melhorScore && score > scoreAtual
                    : score > melhorScore && score > scoreAtual + 0.1f;

                if (aceitaRecomendacao)
                {
                    melhorScore = score;
                    melhorPatio = patio;
                }
            }

            if (melhorPatio != null)
            {
                recomendacoes.Add((moto, moto.Patio, melhorPatio, melhorScore));
            }
        }

        return (recomendacoes.OrderByDescending(r => r.score).ToList(), totalMotosAnalisadas);
    }

    /// <summary>
    /// Cria input para predição ML a partir de uma moto e pátio.
    /// </summary>
    private OtimizacaoDistribuicaoInput CriarInput(Moto moto, Patio patio, List<Patio> todosPatios)
    {
        var motosNoPatio = todosPatios.FirstOrDefault(p => p.PatioId == patio.PatioId)?.Motos.Count ?? 0;
        var capacidadeEstimada = Math.Max(motosNoPatio + 10, 50); // Estimativa conservadora
        var taxaOcupacao = motosNoPatio / (float)capacidadeEstimada;

        // Calcula estatísticas da moto
        var diasDesdeCadastro = (float)(DateTime.UtcNow - moto.DtCadastro).TotalDays;
        var tempoDisponivel = moto.Status == StatusMoto.Disponivel ? diasDesdeCadastro * 0.4f : 0f;
        var tempoAlugada = moto.Status == StatusMoto.Alugada ? diasDesdeCadastro * 0.3f : 0f;
        var tempoManutencao = moto.Status == StatusMoto.EmManutencao ? diasDesdeCadastro * 0.3f : 0f;

        // Calcula taxa de aluguel histórica do pátio (simplificado)
        var taxaAluguelHistorica = patio.Motos.Any() 
            ? patio.Motos.Count(m => m.Status == StatusMoto.Alugada) / (float)Math.Max(patio.Motos.Count, 1)
            : 0.5f;

        var agora = DateTime.UtcNow;

        return new OtimizacaoDistribuicaoInput
        {
            ModeloMoto = moto.Modelo,
            StatusMotoEncoded = (int)moto.Status,
            DiasDesdeCadastro = diasDesdeCadastro,
            TempoMedioDisponivel = tempoDisponivel,
            TempoMedioAlugada = tempoAlugada,
            TempoMedioManutencao = tempoManutencao,
            CategoriaPatioEncoded = (int)patio.Categoria,
            Latitude = (float)patio.Latitude,
            Longitude = (float)patio.Longitude,
            CapacidadePatio = capacidadeEstimada,
            MotosAtuais = motosNoPatio,
            TaxaOcupacao = taxaOcupacao,
            TaxaAluguelHistorica = taxaAluguelHistorica,
            DiaDaSemana = (int)agora.DayOfWeek,
            MesDoAno = agora.Month,
            HoraDoDia = agora.Hour
        };
    }
}


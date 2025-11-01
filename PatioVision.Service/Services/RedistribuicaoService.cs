using Microsoft.EntityFrameworkCore;
using PatioVision.Core.Enums;
using PatioVision.Core.Models.ML;
using PatioVision.Data.Context;

namespace PatioVision.Service.Services;

/// <summary>
/// Serviço que orquestra a coleta de dados e geração de recomendações de redistribuição
/// </summary>
public class RedistribuicaoService
{
    private readonly AppDbContext _context;
    private readonly ML.RedistribuicaoMLService _mlService;

    public RedistribuicaoService(AppDbContext context, ML.RedistribuicaoMLService mlService)
    {
        _context = context;
        _mlService = mlService;
    }

    /// <summary>
    /// Gera recomendações de redistribuição com métricas detalhadas
    /// </summary>
    public async Task<(List<RedistribuicaoOutput> Recomendacoes, Dictionary<string, object> Metricas)> 
        GerarRecomendacoesComMetricasAsync(List<Guid>? motoIds = null, List<Guid>? patioIds = null)
    {
        // Buscar dados para cálculo de métricas (sem Include para evitar ciclos)
        var patiosQuery = _context.Patios.AsQueryable();

        if (patioIds != null && patioIds.Any())
        {
            patiosQuery = patiosQuery.Where(p => patioIds.Contains(p.PatioId));
        }

        var patios = await patiosQuery.AsNoTracking().ToListAsync();
<<<<<<< Updated upstream

        // Buscar motos disponíveis separadamente
=======
<<<<<<< HEAD
        
        Console.WriteLine($"[RedistribuicaoService] Total de pátios encontrados: {patios.Count}");

        // Buscar motos disponíveis separadamente
        // Nota: Não filtrar motos por patioIds aqui, pois queremos analisar todas as motos solicitadas
        // independente do pátio onde estão. O patioIds serve apenas para limitar os pátios de destino.
=======

        // Buscar motos disponíveis separadamente
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
        var motosQuery = _context.Motos
            .Where(m => m.Status == StatusMoto.Disponivel)
            .AsQueryable();

        if (motoIds != null && motoIds.Any())
        {
            motosQuery = motosQuery.Where(m => motoIds.Contains(m.MotoId));
        }

<<<<<<< Updated upstream
=======
<<<<<<< HEAD
        var motosDisponiveis = await motosQuery.AsNoTracking().ToListAsync();
        
        Console.WriteLine($"[RedistribuicaoService] Total de motos disponíveis encontradas: {motosDisponiveis.Count}");
=======
>>>>>>> Stashed changes
        if (patioIds != null && patioIds.Any())
        {
            motosQuery = motosQuery.Where(m => patioIds.Contains(m.PatioId));
        }

        var motosDisponiveis = await motosQuery.AsNoTracking().ToListAsync();
<<<<<<< Updated upstream
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes

        // Calcular métricas atuais
        var totalMotos = motosDisponiveis.Count;
        var totalPatios = patios.Count;
        var mediaMotosPorPatio = totalPatios > 0 ? (float)totalMotos / totalPatios : 0;

        // Calcular distribuição por pátio usando agrupamento
        var distribuicaoPorPatio = motosDisponiveis
            .GroupBy(m => m.PatioId)
            .Select(g => new
            {
                PatioId = g.Key,
                Quantidade = g.Count()
            })
            .ToList();

<<<<<<< Updated upstream
=======
<<<<<<< HEAD
        // Buscar capacidades dos pátios para calcular ocupação percentual
        var patiosComCapacidade = patios.Select(p => new
        {
            p.PatioId,
            p.Capacidade,
            QuantidadeMotos = distribuicaoPorPatio.FirstOrDefault(d => d.PatioId == p.PatioId)?.Quantidade ?? 0
        }).ToList();

        // Calcular ocupação percentual por pátio
        var ocupacoesPercentuais = patiosComCapacidade
            .Where(p => p.Capacidade > 0)
            .Select(p => (float)p.QuantidadeMotos / p.Capacidade)
            .ToList();

        // Calcular desvio padrão atual (baseado em ocupação percentual)
        var mediaOcupacaoPercentual = ocupacoesPercentuais.Any() ? ocupacoesPercentuais.Average() : 0f;
        var desvioPadraoAtual = CalcularDesvioPadrao(ocupacoesPercentuais, mediaOcupacaoPercentual);

        // Contar pátios congestionados (>90% da capacidade) e subutilizados (<50% da capacidade)
        var patiosCongestionados = patiosComCapacidade.Count(p => p.Capacidade > 0 && (float)p.QuantidadeMotos / p.Capacidade > 0.9f);
        var patiosSubutilizados = patiosComCapacidade.Count(p => p.Capacidade > 0 && (float)p.QuantidadeMotos / p.Capacidade < 0.5f);

        // Gerar recomendações
        var recomendacoes = await _mlService.GerarRecomendacoesAsync(motoIds, patioIds);
        
        Console.WriteLine($"[RedistribuicaoService] Total de recomendações geradas: {recomendacoes.Count}");

        // Estimar melhoria no equilíbrio (simulação: assumindo que as top recomendações serão aplicadas)
        // Verificar se há recomendações antes de calcular média para evitar exceção
        var topRecomendacoes = recomendacoes.Take(10).ToList(); // Top 10 recomendações
        
        if (!topRecomendacoes.Any())
        {
            Console.WriteLine("[RedistribuicaoService] AVISO: Nenhuma recomendação foi gerada. Verifique se existem motos disponíveis e pátios cadastrados.");
        }
        
        var melhoriasEsperadas = topRecomendacoes.Any() 
            ? topRecomendacoes.Average(r => r.ImpactoEquilibrio) 
            : 0f;

        var desvioPadraoEstimado = Math.Max(0, desvioPadraoAtual - melhoriasEsperadas);
        
        // Evitar divisão por zero ao calcular melhoria percentual
        var melhoriaPercentual = (desvioPadraoAtual > 0 && mediaMotosPorPatio > 0)
=======
>>>>>>> Stashed changes
        // Calcular desvio padrão atual
        var desvioPadraoAtual = CalcularDesvioPadrao(distribuicaoPorPatio.Select(d => (float)d.Quantidade).ToList(), mediaMotosPorPatio);

        // Contar pátios congestionados e subutilizados
        var patiosCongestionados = distribuicaoPorPatio.Count(d => d.Quantidade > mediaMotosPorPatio * 1.1f);
        var patiosSubutilizados = distribuicaoPorPatio.Count(d => d.Quantidade < mediaMotosPorPatio * 0.9f);

        // Gerar recomendações
        var recomendacoes = await _mlService.GerarRecomendacoesAsync(motoIds, patioIds);

        // Estimar melhoria no equilíbrio (simulação: assumindo que as top recomendações serão aplicadas)
        var melhoriasEsperadas = recomendacoes
            .Take(10) // Top 10 recomendações
            .Average(r => r.ImpactoEquilibrio);

        var desvioPadraoEstimado = Math.Max(0, desvioPadraoAtual - melhoriasEsperadas);
        var melhoriaPercentual = mediaMotosPorPatio > 0 
<<<<<<< Updated upstream
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
            ? (desvioPadraoAtual - desvioPadraoEstimado) / desvioPadraoAtual * 100 
            : 0;

        var metricas = new Dictionary<string, object>
        {
            ["TotalMotos"] = totalMotos,
            ["TotalPatios"] = totalPatios,
            ["MediaMotosPorPatio"] = mediaMotosPorPatio,
            ["DesvioPadraoAtual"] = desvioPadraoAtual,
            ["DesvioPadraoEstimado"] = desvioPadraoEstimado,
            ["MelhoriaEquilibrioPercentual"] = melhoriaPercentual,
            ["PatiosCongestionados"] = patiosCongestionados,
            ["PatiosSubutilizados"] = patiosSubutilizados
        };

        return (recomendacoes, metricas);
    }

    /// <summary>
    /// Calcula desvio padrão de uma distribuição
    /// </summary>
    private float CalcularDesvioPadrao(List<float> valores, float media)
    {
        if (!valores.Any()) return 0;

        var somaQuadrados = valores.Sum(v => Math.Pow(v - media, 2));
        var variancia = somaQuadrados / valores.Count;
        return (float)Math.Sqrt(variancia);
    }
}


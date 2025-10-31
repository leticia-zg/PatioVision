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

        // Buscar motos disponíveis separadamente
        var motosQuery = _context.Motos
            .Where(m => m.Status == StatusMoto.Disponivel)
            .AsQueryable();

        if (motoIds != null && motoIds.Any())
        {
            motosQuery = motosQuery.Where(m => motoIds.Contains(m.MotoId));
        }

        if (patioIds != null && patioIds.Any())
        {
            motosQuery = motosQuery.Where(m => patioIds.Contains(m.PatioId));
        }

        var motosDisponiveis = await motosQuery.AsNoTracking().ToListAsync();

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


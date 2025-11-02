using PatioVision.Core.Models.ML;

namespace PatioVision.API.DTOs;

/// <summary>
/// Response com recomendações de redistribuição e métricas
/// </summary>
public class RedistribuicaoResponse
{
    /// <summary>
    /// Lista de recomendações ordenadas por score
    /// </summary>
    public List<RecomendacaoDetalhada> Recomendacoes { get; set; } = new();

    /// <summary>
    /// Métricas de análise da distribuição atual
    /// </summary>
    public MetricasDistribuicao Metricas { get; set; } = new();

    /// <summary>
    /// Total de recomendações geradas
    /// </summary>
    public int TotalRecomendacoes { get; set; }

    /// <summary>
    /// Mensagem informativa sobre o resultado (ex: quando não há recomendações)
    /// </summary>
    public string? Mensagem { get; set; }
}

/// <summary>
/// Recomendação detalhada de redistribuição
/// </summary>
public class RecomendacaoDetalhada
{
    /// <summary>
    /// ID da moto recomendada para redistribuição
    /// </summary>
    public Guid MotoId { get; set; }

    /// <summary>
    /// Modelo da moto
    /// </summary>
    public string MotoModelo { get; set; } = string.Empty;

    /// <summary>
    /// Placa da moto (se houver)
    /// </summary>
    public string? MotoPlaca { get; set; }

    /// <summary>
    /// ID do pátio atual
    /// </summary>
    public Guid PatioOrigemId { get; set; }

    /// <summary>
    /// Nome do pátio atual
    /// </summary>
    public string PatioOrigemNome { get; set; } = string.Empty;

    /// <summary>
    /// Score de adequação da redistribuição (0-1, onde 1 é melhor)
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    /// ID do pátio recomendado como destino
    /// </summary>
    public Guid PatioDestinoId { get; set; }

    /// <summary>
    /// Nome do pátio recomendado
    /// </summary>
    public string PatioDestinoNome { get; set; } = string.Empty;

    /// <summary>
    /// Motivos da recomendação
    /// </summary>
    public List<string> Motivos { get; set; } = new();

    /// <summary>
    /// Impacto esperado no equilíbrio (diferença antes vs depois)
    /// </summary>
    public float ImpactoEquilibrio { get; set; }

    /// <summary>
    /// Recomendação descritiva da ação a ser tomada
    /// </summary>
    public string Recomendacao { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem informativa quando não há recomendações para esta moto específica
    /// </summary>
    public string? Mensagem { get; set; }
}

/// <summary>
/// Métricas de distribuição atual vs proposta
/// </summary>
public class MetricasDistribuicao
{
    /// <summary>
    /// Número total de motos disponíveis
    /// </summary>
    public int TotalMotos { get; set; }

    /// <summary>
    /// Número total de pátios
    /// </summary>
    public int TotalPatios { get; set; }

    /// <summary>
    /// Média de motos por pátio (distribuição atual)
    /// </summary>
    public float MediaMotosPorPatio { get; set; }

    /// <summary>
    /// Desvio padrão da distribuição atual (quanto maior, mais desequilibrado)
    /// </summary>
    public float DesvioPadraoAtual { get; set; }

    /// <summary>
    /// Desvio padrão estimado após aplicação das recomendações
    /// </summary>
    public float DesvioPadraoEstimado { get; set; }

    /// <summary>
    /// Melhoria percentual no equilíbrio
    /// </summary>
    public float MelhoriaEquilibrioPercentual { get; set; }

    /// <summary>
    /// Número de pátios com ocupação acima da média
    /// </summary>
    public int PatiosCongestionados { get; set; }

    /// <summary>
    /// Número de pátios com ocupação abaixo da média
    /// </summary>
    public int PatiosSubutilizados { get; set; }
}


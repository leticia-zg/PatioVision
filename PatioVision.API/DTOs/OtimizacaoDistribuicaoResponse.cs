namespace PatioVision.API.DTOs;

/// <summary>
/// Resposta com recomendações de otimização de distribuição.
/// </summary>
public class OtimizacaoDistribuicaoResponse
{
    public int TotalMotosAnalisadas { get; set; }
    public int TotalRecomendacoes { get; set; }
    public List<RecomendacaoDistribuicao> Recomendacoes { get; set; } = new();
    public EstatisticasOtimizacao Estatisticas { get; set; } = new();
}

/// <summary>
/// Recomendação individual de redistribuição.
/// </summary>
public class RecomendacaoDistribuicao
{
    public Guid MotoId { get; set; }
    public string ModeloMoto { get; set; } = string.Empty;
    public string StatusAtual { get; set; } = string.Empty;
    public Guid PatioAtualId { get; set; }
    public string PatioAtualNome { get; set; } = string.Empty;
    public Guid PatioRecomendadoId { get; set; }
    public string PatioRecomendadoNome { get; set; } = string.Empty;
    public float ScoreAdequacao { get; set; }
    public float MelhoriaEsperada { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public List<string> Beneficios { get; set; } = new();
}

/// <summary>
/// Estatísticas agregadas da otimização.
/// </summary>
public class EstatisticasOtimizacao
{
    public string BeneficioEsperado { get; set; } = string.Empty;
    public float ReducaoTempoMedioAtendimento { get; set; }
    public float MelhoriaBalanceamentoPatio { get; set; }
    public int PatiosAfetados { get; set; }
}


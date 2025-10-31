namespace PatioVision.Core.Models.ML;

/// <summary>
/// Modelo de saída da predição de otimização de distribuição.
/// </summary>
public class OtimizacaoDistribuicaoOutput
{
    public float ScoreAdequacao { get; set; }
    public float ProbabilidadeSucesso { get; set; }
}


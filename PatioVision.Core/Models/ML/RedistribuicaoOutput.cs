namespace PatioVision.Core.Models.ML;

/// <summary>
/// Resultado da predição de redistribuição
/// </summary>
public class RedistribuicaoOutput
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
    /// ID do pátio de origem
    /// </summary>
    public Guid PatioOrigemId { get; set; }

    /// <summary>
    /// Nome do pátio de origem
    /// </summary>
    public string PatioOrigemNome { get; set; } = string.Empty;

    /// <summary>
    /// Score de adequação da redistribuição (0-1, onde 1 é melhor)
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    /// Id do pátio recomendado como destino
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
}


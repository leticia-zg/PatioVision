namespace PatioVision.API.DTOs;

/// <summary>
/// Request para otimização de distribuição de motos.
/// </summary>
public class OtimizacaoDistribuicaoRequest
{
    /// <summary>
    /// IDs das motos a serem analisadas. Se vazio, analisa todas as motos disponíveis.
    /// </summary>
    public List<Guid>? MotoIds { get; set; }

    /// <summary>
    /// Se verdadeiro, considera apenas motos com status Disponível.
    /// </summary>
    public bool ApenasDisponiveis { get; set; } = true;

    /// <summary>
    /// Se verdadeiro, gera recomendações mesmo para motos já bem posicionadas.
    /// </summary>
    public bool IncluirTodasMotos { get; set; } = false;
}


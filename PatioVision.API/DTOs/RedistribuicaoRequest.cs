namespace PatioVision.API.DTOs;

/// <summary>
/// Request para geração de recomendações de redistribuição
/// </summary>
public class RedistribuicaoRequest
{
    /// <summary>
    /// Lista opcional de IDs de motos específicas para análise.
    /// Se vazio, analisa todas as motos disponíveis.
    /// </summary>
    public List<Guid>? MotoIds { get; set; }

    /// <summary>
    /// Lista opcional de IDs de pátios específicos para considerar como destino.
    /// Se vazio, considera todos os pátios disponíveis.
    /// </summary>
    public List<Guid>? PatioIds { get; set; }
}


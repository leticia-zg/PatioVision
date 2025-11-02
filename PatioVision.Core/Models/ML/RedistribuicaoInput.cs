using Microsoft.ML.Data;

namespace PatioVision.Core.Models.ML;

/// <summary>
/// Features de entrada para o modelo ML de redistribuição
/// </summary>
public class RedistribuicaoInput
{
    public float PatioAtualId { get; set; }

    public float PatioDestinoId { get; set; }

    public float QuantidadeMotosAtual { get; set; }

    public float QuantidadeMotosDestino { get; set; }

    public float CategoriaPatioAtual { get; set; } // Enum convertido para float

    public float CategoriaPatioDestino { get; set; }

    public float StatusMoto { get; set; } // Enum convertido para float

    public float DistanciaKm { get; set; }

    public float TaxaOcupacaoAtual { get; set; }

    public float TaxaOcupacaoDestino { get; set; }

    public float CapacidadePatioAtual { get; set; }

    public float CapacidadePatioDestino { get; set; }

    public float PercentualOcupacaoAtual { get; set; } // quantidadeMotos / capacidade (0-1)

    public float PercentualOcupacaoDestino { get; set; }

    public float EspacoDisponivelDestino { get; set; } // capacidade - quantidade atual

    public float DiferencaEquilibrio { get; set; } // Diferença entre ocupação atual e ideal

    public float MediaGeralMotosPorPatio { get; set; }

    [ColumnName("Label")]
    public float ScoreRedistribuicao { get; set; } // Score de 0-1 indicando quão boa é a redistribuição
}


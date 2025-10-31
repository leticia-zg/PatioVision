namespace PatioVision.Core.Models.ML;

/// <summary>
/// Modelo de entrada para treinamento e predição de otimização de distribuição.
/// Representa as features que influenciam a adequação de uma moto em um pátio.
/// </summary>
public class OtimizacaoDistribuicaoInput
{
    // Features da Moto
    public string ModeloMoto { get; set; } = string.Empty;
    public int StatusMotoEncoded { get; set; } // 0=Disponivel, 1=EmManutencao, 2=Alugada
    public float DiasDesdeCadastro { get; set; }
    public float TempoMedioDisponivel { get; set; }
    public float TempoMedioAlugada { get; set; }
    public float TempoMedioManutencao { get; set; }

    // Features do Pátio
    public int CategoriaPatioEncoded { get; set; } // 0=SemPlaca, 1=Manutencao, 2=Aluguel
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public float CapacidadePatio { get; set; } // Total de motos que o pátio pode comportar
    public float MotosAtuais { get; set; } // Quantas motos estão atualmente no pátio
    public float TaxaOcupacao { get; set; } // MotosAtuais / CapacidadePatio
    public float TaxaAluguelHistorica { get; set; } // % de motos alugadas vs disponíveis historicamente

    // Features Temporais
    public int DiaDaSemana { get; set; } // 0=Domingo, 6=Sábado
    public int MesDoAno { get; set; } // 1-12
    public int HoraDoDia { get; set; } // 0-23

    // Label (para treinamento)
    public float ScoreAdequacao { get; set; } // 0.0 a 1.0 - quão adequada é a moto neste pátio
}


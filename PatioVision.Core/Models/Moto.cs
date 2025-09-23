namespace PatioVision.Core.Models;

using System;
using System.ComponentModel.DataAnnotations;
using PatioVision.Core.Enums;

public class Moto
{
    [Key]
    public Guid MotoId { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50)]
    public string Modelo { get; set; }

    [StringLength(10)]
    public string? Placa { get; set; } // Caso tenha motos sem placa (como apontado no Kickoff)

    [Required]
    public StatusMoto Status { get; set; } // Enum para definir o estado da moto: Disponivel, EmManutencaom, Alugada

    [Required]
    public Guid PatioId { get; set; } // Relacionamento com o Pátio

    public Patio? Patio { get; set; }

    [Required]
    public Guid DispositivoIotId { get; set; } // Relacionamento com o IoT da moto

    public DispositivoIoT? Dispositivo { get; set; }

    // Campos de auditoria
    public DateTime DtCadastro { get; set; }
    public DateTime? DtAtualizacao { get; set; }
}

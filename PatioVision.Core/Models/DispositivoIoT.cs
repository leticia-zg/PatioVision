namespace PatioVision.Core.Models;

using System;
using System.ComponentModel.DataAnnotations;
using PatioVision.Core.Enums;

public class DispositivoIoT
{
    [Key]
    public Guid DispositivoIotId { get; set; } = Guid.NewGuid();

    [Required]
    public TipoDispositivo Tipo { get; set; } // Enum para diferenciar IoT de Moto e Pátio

    [StringLength(255)]
    public string UltimaLocalizacao { get; set; } // Dados de localização que foram coletados

    public DateTime UltimaAtualizacao { get; set; } = DateTime.UtcNow;
}

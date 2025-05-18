namespace PatioVision.Core.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PatioVision.Core.Enums;

public class Patio
{
    [Key]
    public Guid PatioId { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Nome { get; set; }

    [Required]
    public CategoriaPatio Categoria { get; set; } // Enum para definir o tipo do pátio

    [Required]
    public decimal Latitude { get; set; }

    [Required]
    public decimal Longitude { get; set; }

    [Required]
    public Guid DispositivoIotId { get; set; } // Relacionamento com o IoT do pátio

    public DispositivoIoT? Dispositivo { get; set; }

    public ICollection<Moto> Motos { get; set; } = new List<Moto>();
}

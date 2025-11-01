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
    public CategoriaPatio Categoria { get; set; } // Enum para definir o tipo do p�tio

    [Required]
    public decimal Latitude { get; set; }

    [Required]
    public decimal Longitude { get; set; }

    [Required]
    [Range(1, 1000, ErrorMessage = "A capacidade deve estar entre 1 e 1000 motos")]
    public int Capacidade { get; set; } // Capacidade máxima de motos que o pátio pode comportar

    [Required]
    public Guid DispositivoIotId { get; set; } // Relacionamento com o IoT do p�tio

    public DispositivoIoT? Dispositivo { get; set; }

    public ICollection<Moto> Motos { get; set; } = new List<Moto>();

    // Campos de auditoria
    // DtCadastro: data de cria��o (UTC). Sempre setado no service ao criar.
    public DateTime DtCadastro { get; set; }

    // DtAtualizacao: ultima atualiza��o (UTC). Atualizado no Update.
    public DateTime? DtAtualizacao { get; set; }
}

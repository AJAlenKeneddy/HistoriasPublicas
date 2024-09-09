using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StoryBlazeServer.Models;

public partial class Historia
{
    public int HistoriaId { get; set; }
    [StringLength(30, ErrorMessage = "El resumen no puede tener más de 30 caracteres.")]
    public string Titulo { get; set; } = null!;

    [StringLength(40, ErrorMessage = "El resumen no puede tener más de 40 caracteres.")]
    public string? Resumen { get; set; }

    public int? UsuarioCreadorId { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public string? Estado { get; set; }

    public bool Eliminado { get; set; }

    public int? CategoriaId { get; set; }

    public virtual Categoria? Categoria { get; set; }

    public virtual ICollection<Fragmento> Fragmentos { get; set; } = new List<Fragmento>();

    public virtual Usuario? UsuarioCreador { get; set; }
}

using System;
using System.Collections.Generic;

namespace StoryBlaze.Models;

public partial class Historia
{
    public int HistoriaId { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Resumen { get; set; }

    public int? UsuarioCreadorId { get; set; }

    public DateOnly? FechaCreacion { get; set; }

    public string? Estado { get; set; }

    public bool Eliminado { get; set; }

    public virtual ICollection<Fragmento> Fragmentos { get; set; } = new List<Fragmento>();

    public virtual Usuario? UsuarioCreador { get; set; }
}

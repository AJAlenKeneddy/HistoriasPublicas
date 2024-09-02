using System;
using System.Collections.Generic;

namespace StoryBlaze.Models;

public partial class Fragmento
{
    public int FragmentoId { get; set; }

    public int? HistoriaId { get; set; }

    public int? UsuarioId { get; set; }

    public string Contenido { get; set; } = null!;

    public DateOnly? FechaCreacionFrag { get; set; }

    public bool Eliminado { get; set; }

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual Historia? Historia { get; set; }

    public virtual Usuario? Usuario { get; set; }

    public virtual ICollection<Voto> Votos { get; set; } = new List<Voto>();
}

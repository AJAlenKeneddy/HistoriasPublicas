using System;
using System.Collections.Generic;

namespace StoryBlazeServer.Models;

public partial class Comentario
{
    public int ComentarioId { get; set; }

    public int? FragmentoId { get; set; }

    public int? UsuarioId { get; set; }

    public string? Comentario1 { get; set; }

    public DateTime? FechaComentario { get; set; }

    public bool Eliminado { get; set; }

    public virtual Fragmento? Fragmento { get; set; }

    public virtual Usuario? Usuario { get; set; }
}

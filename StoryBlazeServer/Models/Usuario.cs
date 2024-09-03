using System;
using System.Collections.Generic;

namespace StoryBlazeServer.Models;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string ContraseñaHash { get; set; } = null!;

    public DateTime? FechaRegistro { get; set; }

    public string? CodigoVerificacion { get; set; }

    public DateTime? FechaExpiracionCodigo { get; set; }

    public bool Verificado { get; set; }

    public string? CodigoRecuperacion { get; set; }

    public DateTime? FechaExpiracionCodigoRecuperacion { get; set; }

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual ICollection<Fragmento> Fragmentos { get; set; } = new List<Fragmento>();

    public virtual ICollection<Historia> Historia { get; set; } = new List<Historia>();

    public virtual ICollection<Voto> Votos { get; set; } = new List<Voto>();
}

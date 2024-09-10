using System;
using System.Collections.Generic;

namespace StoryBlazeServer.Models
{
    public partial class Usuario
    {
        public Usuario()
        {
            Comentarios = new HashSet<Comentario>();
            Fragmentos = new HashSet<Fragmento>();
            Historia = new HashSet<Historia>();
            Votos = new HashSet<Voto>();
        }

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

        public virtual ICollection<Comentario> Comentarios { get; set; }
        public virtual ICollection<Fragmento> Fragmentos { get; set; }
        public virtual ICollection<Historia> Historia { get; set; }
        public virtual ICollection<Voto> Votos { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace StoryBlazeServer.Models
{
    public partial class Fragmento
    {
        public Fragmento()
        {
            Comentarios = new HashSet<Comentario>();
            Votos = new HashSet<Voto>();
        }

        public int FragmentoId { get; set; }
        public int? HistoriaId { get; set; }
        public int? UsuarioId { get; set; }
        public string Contenido { get; set; } = null!;
        public DateTime? FechaCreacionFrag { get; set; }
        public bool Eliminado { get; set; }
        public int? TotalVotos { get; set; }

        public virtual Historia? Historia { get; set; }
        public virtual Usuario? Usuario { get; set; }
        public virtual ICollection<Comentario> Comentarios { get; set; }
        public virtual ICollection<Voto> Votos { get; set; }
    }
}

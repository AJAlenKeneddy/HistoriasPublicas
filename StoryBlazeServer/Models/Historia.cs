using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StoryBlazeServer.Models
{
    public partial class Historia
    {
        public Historia()
        {
            Fragmentos = new HashSet<Fragmento>();
        }
        
        public int HistoriaId { get; set; }
       
        public string Titulo { get; set; } = null!;
        
        public string? Resumen { get; set; }
        
        public int? UsuarioCreadorId { get; set; }
        
        public DateTime? FechaCreacion { get; set; }
        
        public string? Estado { get; set; }
        
        public bool Eliminado { get; set; }
        
        public int? CategoriaId { get; set; }

        public virtual Categorium? Categoria { get; set; }
        public virtual Usuario? UsuarioCreador { get; set; }
        public virtual ICollection<Fragmento> Fragmentos { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace StoryBlazeServer.Models
{
    public class sp_ListarHistorias
    {
        [Key]
        public int HistoriaID { get; set; }
        public string? Titulo { get; set; }
        public string? Resumen { get; set; }
        public DateTime? FechaCreacion { get; set; } 
        public string? NombreUsuario { get; set; }
        public string? Estado { get; set; }
    }
}

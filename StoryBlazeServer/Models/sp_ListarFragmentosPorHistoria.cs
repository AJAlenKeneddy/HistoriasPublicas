using System.ComponentModel.DataAnnotations;

namespace StoryBlazeServer.Models
{
    public class sp_ListarFragmentosPorHistoria
    {
        [Key]
        public int FragmentoID { get; set; }
        public int UsuarioID { get; set; }
        public string Contenido { get; set; } = null!;
        public DateTime? FechaCreacionFrag { get; set; }
    }
}

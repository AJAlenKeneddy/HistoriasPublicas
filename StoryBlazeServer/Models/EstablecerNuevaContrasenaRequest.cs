namespace StoryBlazeServer.Models
{
    public class EstablecerNuevaContrasenaRequest
    {
        public string Correo { get; set; }
        public string CodigoRecuperacion { get; set; }
        public string NuevaContrasena { get; set; }
    }
}

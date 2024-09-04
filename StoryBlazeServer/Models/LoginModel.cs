using System.ComponentModel.DataAnnotations;

namespace StoryBlazeServer.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        public string Clave { get; set; } = string.Empty;
    }
}

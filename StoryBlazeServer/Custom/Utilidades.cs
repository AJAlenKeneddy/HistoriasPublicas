using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using StoryBlazeServer.Models;


namespace StoryBlazeServer.Custom
{
    public class Utilidades
    {
        private readonly IConfiguration _configuration;
        public Utilidades(IConfiguration configuration) { 
            _configuration = configuration;
        }
        public string encriptarSHA256(string texto)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(texto));
                StringBuilder builder= new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                { 
                    builder.Append(bytes[i].ToString("X2"));

                }
                return builder.ToString();


            }
        }
    //    public string generarJWT(Usuario usuario)
    //    {
    //        var claims = new[]
    //        {
    //    new Claim(JwtRegisteredClaimNames.Sub, usuario.NombreUsuario),
    //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    //    new Claim("UserCreadorId", usuario.UsuarioId.ToString()), // Agrega el UserId como un claim
    //    // Puedes agregar otros claims si es necesario
    //};

    //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
    //        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    //        var token = new JwtSecurityToken(
    //            issuer: _configuration["Jwt:Issuer"],
    //            audience: _configuration["Jwt:Audience"],
    //            claims: claims,
    //            expires: DateTime.UtcNow.AddHours(1),
    //            signingCredentials: creds);

    //        return new JwtSecurityTokenHandler().WriteToken(token);
    //    }





    }
}

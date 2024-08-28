using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryBlaze.Models;
using Microsoft.EntityFrameworkCore;
using WEBAPIGMINGENIEROSHTTPS.Custom;
using WEBAPIGMINGENIEROSHTTPS.Models.Services;

using Microsoft.Data.SqlClient;

namespace StoryBlaze.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AccesoController : ControllerBase
    {
        private readonly StoryBlazeContext db;
        private readonly Utilidades util;
        private readonly EmailService emailService;
        public AccesoController(StoryBlazeContext storyBlazeContext, Utilidades utilidades, EmailService emailService)
        {
            db = storyBlazeContext;
            util = utilidades;
            this.emailService = emailService;
        }






        [HttpPost]
        [Route("Registrarse")]
        public async Task<IActionResult> Registrarse([FromBody] RegistrarseRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Nombre) || string.IsNullOrEmpty(request.Correo) || string.IsNullOrEmpty(request.Clave))
            {
                return BadRequest(new RegistrarseResponse { IsSuccess = false, Message = "Todos los campos son requeridos." });
            }

            try
            {
                var codigoVerificacion = new Random().Next(10000, 99999).ToString(); // Código de 5 dígitos
                var modeloUsuario = new Usuario
                {
                    NombreUsuario = request.Nombre,
                    Correo = request.Correo,
                    ContraseñaHash = util.encriptarSHA256(request.Clave),
                    CodigoVerificacion = codigoVerificacion,
                    FechaExpiracionCodigo = DateTime.Now.AddHours(24), // Expiración de 24 horas
                    Verificado = false
                };

                await db.Usuarios.AddAsync(modeloUsuario);
                await db.SaveChangesAsync();

                var emailBody = $@"
            <html>
            <body>
                <h2>Verificación de Correo</h2>
                <p>Tu código de verificación es:
                <br>
                <strong>{codigoVerificacion}</strong></p>
            </body>
            </html>
        ";

                var emailSent = await emailService.SendEmailAsync(
                    "Remitente",
                    "alenaguilar24@gmail.com",
                    "Destinatario",
                    request.Correo,
                    "Código de Verificación",
                    emailBody);

                if (emailSent)
                {
                    return Ok(new RegistrarseResponse { IsSuccess = true, Message = "Registro exitoso. Verifica tu correo electrónico." });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new RegistrarseResponse { IsSuccess = false, Message = "Error al enviar el correo. Inténtalo de nuevo." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                var sqlException = dbEx.InnerException as SqlException;
                if (sqlException != null)
                {
                    if (sqlException.Number == 2627) // Violación de clave única
                    {
                        if (sqlException.Message.Contains("UQ__Usuarios__60695A1984046E71"))
                        {
                            return StatusCode(StatusCodes.Status409Conflict, new
                            {
                                IsSuccess = false,
                                Message = "El correo electrónico ya está en uso.",
                                Details = sqlException.Message
                            });
                        }
                    }
                }

                // Manejo de otros errores de actualización de base de datos
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    IsSuccess = false,
                    Message = "Error al guardar los cambios en la base de datos.",
                    Details = dbEx.Message
                });
            }
            catch (Exception ex)
            {
                // Manejo de otros errores
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    IsSuccess = false,
                    Message = "Error interno del servidor.",
                    Details = ex.Message
                });
            }
        }

        

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Correo) || string.IsNullOrEmpty(loginRequest.Clave))
                return BadRequest(new LoginResponse { IsSuccess = false, Message = "Correo y clave son requeridos." });


            try
            {
                var claveEncriptada = util.encriptarSHA256(loginRequest.Clave);
                var usuarioEncontrado = await db.Usuarios
                    .Where(u => u.Correo == loginRequest.Correo && u.ContraseñaHash == claveEncriptada)
                    .FirstOrDefaultAsync();

                if (usuarioEncontrado == null)
                {
                    return Unauthorized(new LoginResponse { IsSuccess = false, Message = "Credenciales inválidas." });
                }

                // Generar JWT token
                var token = util.generarJWT(usuarioEncontrado);

                // Preparar el cuerpo del correo con el token
                var emailBody = $@"
                     <html>
                     <body>
                    <h2>Token de Inicio de Sesión</h2>
                    <p>Tu token JWT es:
                    <br>
                    <strong>{token}</strong></p>
                    </body>
                    </html>
                     ";

                var emailSent = await emailService.SendEmailAsync(
                    "Remitente",
                    "alenaguilar24@gmail.com",
                    "Destinatario",
                    loginRequest.Correo,
                    "Token de Inicio de Sesión",
                    emailBody);

                if (emailSent)
                {
                    return Ok(new LoginResponse { IsSuccess = true, Token = token, Message = "Envío exitoso. Verifica tu correo electrónico." });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponse { IsSuccess = false, Message = "Error al enviar el correo. Inténtalo de nuevo." });
                }
            }
            catch (Exception ex)
            {
                // Registra el error en un log
                // Logger.LogError(ex, "Error en el proceso de inicio de sesión");

                return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponse { IsSuccess = false, Message = "Error interno del servidor." });
            }
        }

        

        [HttpPost]
        [Route("VerificarCuenta")]
        public async Task<IActionResult> VerificarCuenta([FromBody] VerificarCuentaRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Correo) || string.IsNullOrEmpty(request.CodigoVerificacion))            
                return BadRequest(new { IsSuccess = false, Message = "Correo y código de verificación son requeridos." });
            

            try
            {
                var usuario = await db.Usuarios
                    .Where(u => u.Correo == request.Correo)
                    .FirstOrDefaultAsync();

                if (usuario == null)
                    return NotFound(new { IsSuccess = false, Message = "Usuario no encontrado." });
                

                if (usuario.CodigoVerificacion != request.CodigoVerificacion)                
                    return BadRequest(new { IsSuccess = false, Message = "Código de verificación incorrecto." });
                

                if (usuario.FechaExpiracionCodigo < DateTime.Now)                
                    return BadRequest(new { IsSuccess = false, Message = "El código de verificación ha expirado." });
                

                usuario.Verificado = true;
                usuario.CodigoVerificacion = null; // Limpiar el código de verificación después de la verificación
                usuario.FechaExpiracionCodigo = null; // Limpiar la fecha de expiración

                db.Usuarios.Update(usuario);
                await db.SaveChangesAsync();

                return Ok(new { IsSuccess = true, Message = "Cuenta verificada exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    IsSuccess = false,
                    Message = "Error interno del servidor.",
                    Details = ex.Message
                });
            }
        }



    }
}

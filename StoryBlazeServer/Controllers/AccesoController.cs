using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryBlazeServer.Models;
using Microsoft.EntityFrameworkCore;
using WEBAPIGMINGENIEROSHTTPS.Custom;
using WEBAPIGMINGENIEROSHTTPS.Models.Services;

using Microsoft.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace StoryBlazeServer.Controllers
{



    /* 
     Configuracion del Enpoint y llamado de Contexto de la BD ademas 
     de Utilidades y el Servicio de Envio de Correo
    */
    [Route("api/[controller]")]
    [ApiController]
    public class AccesoController : ControllerBase
    {
        private readonly StoryBlazeServerContext db;
        private readonly Utilidades util;
        private readonly EmailService emailService;
        public AccesoController(StoryBlazeServerContext StoryBlazeServerContext, Utilidades utilidades, EmailService emailService)
        {
            db = StoryBlazeServerContext;
            util = utilidades;
            this.emailService = emailService;
        }





        /*
         Endpoint para Registrar Usuarios ademas de generacion de un codigo de Verificacion de 
         5 digitos y su envio a su respectivo correo el cual hace uso de Utilidades de Hash Contraseña y EmailService 
        */
        [HttpPost]
        [Route("Registrarse")]
        public async Task<IActionResult> Registrarse([FromBody] RegistrarseRequest request)
        {
            // Validación de campos requeridos
            if (request == null || string.IsNullOrEmpty(request.Nombre) || string.IsNullOrEmpty(request.Correo) || string.IsNullOrEmpty(request.Clave))
                return BadRequest(new RegistrarseResponse { IsSuccess = false, Message = "Todos los campos son requeridos." });

            try
            {
                // Validar si el correo ya está en uso antes de insertar
                var usuarioExistente = await db.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Correo);
                if (usuarioExistente != null)
                {
                    return StatusCode(StatusCodes.Status409Conflict, new
                    {
                        IsSuccess = false,
                        Message = "El correo electrónico ya está en uso."
                    });
                }

                // Generación del código de verificación
                var codigoVerificacion = new Random().Next(10000, 99999).ToString();

                // Crear el modelo de usuario
                var modeloUsuario = new Usuario
                {
                    NombreUsuario = request.Nombre,
                    Correo = request.Correo,
                    ContraseñaHash = util.encriptarSHA256(request.Clave),
                    CodigoVerificacion = codigoVerificacion,
                    FechaExpiracionCodigo = DateTime.Now.AddHours(1),
                    Verificado = false
                };

                // Intentar guardar el nuevo usuario
                await db.Usuarios.AddAsync(modeloUsuario);
                await db.SaveChangesAsync();

                // Enviar correo electrónico con el código de verificación
                var emailBody = $@"
        <html>
        <body>
            <h2>Verificación de Correo</h2>
            <p>Tu código de verificación es:
            <br>
            <strong>{codigoVerificacion}</strong></p>
        </body>
        </html>";

                var emailSent = await emailService.SendEmailAsync(
                    "Remitente",
                    "alenaguilar24@gmail.com",
                    "Destinatario",
                    request.Correo,
                    "Código de Verificación",
                    emailBody);

                // Verificar si el correo fue enviado con éxito
                if (emailSent)
                    return Ok(new RegistrarseResponse { IsSuccess = true, Message = "Registro exitoso. Verifica tu correo electrónico." });
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new RegistrarseResponse { IsSuccess = false, Message = "Error al enviar el correo. Inténtalo de nuevo." });
            }
            catch (DbUpdateException dbEx)
            {
                // Manejo de errores de base de datos
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    IsSuccess = false,
                    Message = "Error al guardar los cambios en la base de datos.",
                    Details = dbEx.Message
                });
            }
            catch (Exception ex)
            {
                // Manejo de cualquier otra excepción
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    IsSuccess = false,
                    Message = "Error interno del servidor.",
                    Details = ex.Message
                });
            }
        }



        /*
         Endpoint para poder Iniciar Session con utilidad de DesHash de Contraseña     
         */
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
                    return Unauthorized(new LoginResponse { IsSuccess = false, Message = "Credenciales inválidas." });
                else if (!usuarioEncontrado.Verificado)
                    return Unauthorized(new LoginResponse { IsSuccess = false, Message = "El correo no ha sido verificado. Por favor, verifica tu correo antes de iniciar sesión." });

                var token = util.generarJWT(usuarioEncontrado);

                return Ok(new LoginResponse
                {
                    IsSuccess = true,
                    Token = token,
                    Message = "Inicio de sesión exitoso."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponse
                {
                    IsSuccess = false,
                    Message = ("Error interno del servidor.")
                    
                });
            }
        }



        /*
         Endpoint Para Verificar Cuenta Mediante el Correo Especificado ademas 
         del Codigo que se envio al Correo
         */
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
                usuario.CodigoVerificacion = null; 
                usuario.FechaExpiracionCodigo = null; 

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


        /*
         Endpoint para Solicitar un Nuevo codigo de Verificacion que se envia al Correo Correspondiente
         */
        [HttpPost]
        [Route("ActualizarCodigoVerificacion")]
        public async Task<IActionResult> ActualizarCodigoVerificacion([FromBody] SolicitarNuevoCodigoRequest request)
        {
            if (string.IsNullOrEmpty(request.Correo))
            {
                return BadRequest(new { IsSuccess = false, Message = "El correo es requerido." });
            }

            try
            {
                
                var usuarioExistente = await db.Usuarios
                    .Where(u => u.Correo == request.Correo)
                    .FirstOrDefaultAsync();

                if (usuarioExistente == null)
                {
                    return NotFound(new { IsSuccess = false, Message = "Usuario no encontrado." });
                }

                
                if (usuarioExistente.Verificado)
                {
                    return BadRequest(new { IsSuccess = false, Message = "El usuario ya está verificado." });
                }

                
                var nuevoCodigo = new Random().Next(10000, 99999).ToString();
                usuarioExistente.CodigoVerificacion = nuevoCodigo;
                usuarioExistente.FechaExpiracionCodigo = DateTime.Now.AddHours(1);



                
                db.Usuarios.Update(usuarioExistente);
                await db.SaveChangesAsync();

                
                var emailBody = $@"
            <html>
            <body>
                <h2>Verificación de Correo</h2>
                <p>Tu nuevo código de verificación es:
                <br>
                <strong>{nuevoCodigo}</strong></p>
            </body>
            </html>
        ";

                var emailSent = await emailService.SendEmailAsync(
                    "Remitente",
                    "alenaguilar24@gmail.com",
                    "Destinatario",
                    request.Correo,
                    "Nuevo Código de Verificación",
                    emailBody);

                if (emailSent)
                {
                    return Ok(new { IsSuccess = true, Message = "Se ha enviado un nuevo código de verificación a tu correo." });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al enviar el correo. Inténtalo de nuevo." });
                }
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



        /*
         Endpoint que realiza el envio de un Correo para poder cambiar la contraseña del usuario en Especifico
         */
        [HttpPost]
        [Route("SolicitarCambioContrasena")]
        public async Task<IActionResult> SolicitarCambioContrasena([FromBody] SolicitarCambioContrasenaRequest request)
        {
            if (string.IsNullOrEmpty(request.Correo))
                return BadRequest(new { IsSuccess = false, Message = "El correo es requerido." });

            try
            {
                
                var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Correo);
                if (usuario == null)
                    return NotFound(new { IsSuccess = false, Message = "Usuario no encontrado." });

                
                var codigoRecuperacion = new Random().Next(10000, 99999).ToString();
                usuario.CodigoRecuperacion = codigoRecuperacion;
                usuario.FechaExpiracionCodigoRecuperacion = DateTime.Now.AddHours(1); 

                
                db.Usuarios.Update(usuario);
                await db.SaveChangesAsync();

                
                var enlaceRecuperacion = Url.Action("RecuperarContrasena", "Acceso", new { codigo = codigoRecuperacion, correo = request.Correo }, Request.Scheme);
                var emailBody = $@"
                                <html>
                                <body>
                                    <h2>Recuperación de Contraseña</h2>
                                    <p>Para restablecer tu contraseña, haz clic en el siguiente botón:</p>
                                    <a href='{enlaceRecuperacion}' style='background-color: #4CAF50; color: white; padding: 15px 20px; text-align: center; text-decoration: none; display: inline-block; margin: 4px 2px; cursor: pointer;'>Restablecer Contraseña</a>
                                </body>
                                </html>";

                var emailSent = await emailService.SendEmailAsync(
                    "Remitente",
                    "alenaguilar24@gmail.com",
                    "Destinatario",
                    request.Correo,
                    "Solicitud de Cambio de Contraseña",
                    emailBody);

                if (emailSent)
                    return Ok(new { IsSuccess = true, Message = "Se ha enviado un enlace para restablecer la contraseña a tu correo." });
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al enviar el correo. Inténtalo de nuevo." });
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


        [HttpGet]
        [Route("RecuperarContrasena")]
        public async Task<IActionResult> RecuperarContrasena([FromQuery] string codigo, [FromQuery] string correo)
        {
            if (string.IsNullOrEmpty(codigo) || string.IsNullOrEmpty(correo))
                return BadRequest(new { IsSuccess = false, Message = "Código y correo son requeridos." });

            try
            {
                var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
                if (usuario == null)
                    return NotFound(new { IsSuccess = false, Message = "Usuario no encontrado." });

                if (usuario.CodigoRecuperacion != codigo)
                    return BadRequest(new { IsSuccess = false, Message = "Código de recuperación incorrecto." });

                if (usuario.FechaExpiracionCodigoRecuperacion < DateTime.Now)
                    return BadRequest(new { IsSuccess = false, Message = "El código de recuperación ha expirado." });

                // Aquí podrías redirigir al usuario a una página para ingresar la nueva contraseña
                // En una API, podrías simplemente enviar un mensaje indicando que el enlace es válido

                return Ok(new { IsSuccess = true, Message = "Código de recuperación válido. Por favor, proporciona una nueva contraseña." });
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


        [HttpPost]
        [Route("EstablecerNuevaContrasena")]
        public async Task<IActionResult> EstablecerNuevaContrasena([FromBody] EstablecerNuevaContrasenaRequest request)
        {
            if (string.IsNullOrEmpty(request.Correo) || string.IsNullOrEmpty(request.CodigoRecuperacion) || string.IsNullOrEmpty(request.NuevaContrasena))
                return BadRequest(new { IsSuccess = false, Message = "Correo, código de recuperación y nueva contraseña son requeridos." });

            try
            {
                var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Correo);
                if (usuario == null)
                    return NotFound(new { IsSuccess = false, Message = "Usuario no encontrado." });

                if (usuario.CodigoRecuperacion != request.CodigoRecuperacion)
                    return BadRequest(new { IsSuccess = false, Message = "Código de recuperación incorrecto." });

                if (usuario.FechaExpiracionCodigoRecuperacion < DateTime.Now)
                    return BadRequest(new { IsSuccess = false, Message = "El código de recuperación ha expirado." });

                // Establecer la nueva contraseña
                usuario.ContraseñaHash = util.encriptarSHA256(request.NuevaContrasena);
                usuario.CodigoRecuperacion = null;
                usuario.FechaExpiracionCodigoRecuperacion = null;

                db.Usuarios.Update(usuario);
                await db.SaveChangesAsync();

                return Ok(new { IsSuccess = true, Message = "Contraseña actualizada exitosamente." });
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



        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("ObtenerInformacion")]
        public async Task<IActionResult> ObtenerInformacion()
        {
            // Se asume que el usuario está autenticado y el token JWT ha sido validado para obtener el correo.
            var correoUsuario = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(correoUsuario))
                return Unauthorized(new { IsSuccess = false, Message = "Usuario no autenticado." });

            try
            {
                var usuario = await db.Usuarios
                    .Where(u => u.Correo == correoUsuario)
                    .FirstOrDefaultAsync();

                if (usuario == null)
                    return NotFound(new { IsSuccess = false, Message = "Usuario no encontrado." });

                return Ok(new
                {
                    IsSuccess = true,
                    Usuario = new
                    {
                        Nombre = usuario.NombreUsuario,
                        Correo = usuario.Correo,
                        Verificado = usuario.Verificado
                    }
                });
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


        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("CerrarSesion")]
        public IActionResult CerrarSesion()
        {
            // Aquí puedes implementar lógica para invalidar el token JWT, si estás usando uno.
            return Ok(new { IsSuccess = true, Message = "Sesión cerrada exitosamente." });
        }








    }
}

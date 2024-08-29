using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryBlaze.Models;
using Microsoft.EntityFrameworkCore;
using WEBAPIGMINGENIEROSHTTPS.Custom;
using WEBAPIGMINGENIEROSHTTPS.Models.Services;

using Microsoft.Data.SqlClient;

namespace StoryBlaze.Controllers
{



    /* 
     Configuracion del Enpoint y llamado de Contexto de la BD ademas 
     de Utilidades y el Servicio de Envio de Correo
    */
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





        /*
         Endpoint para Registrar Usuarios ademas de generacion de un codigo de Verificacion de 
         5 digitos y su envio a su respectivo correo el cual hace uso de Utilidades de Hash Contraseña y EmailService 
        */
        [HttpPost]
        [Route("Registrarse")]
        public async Task<IActionResult> Registrarse([FromBody] RegistrarseRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Nombre) || string.IsNullOrEmpty(request.Correo) || string.IsNullOrEmpty(request.Clave))
                return BadRequest(new RegistrarseResponse { IsSuccess = false, Message = "Todos los campos son requeridos." });

            try
            {
                var codigoVerificacion = new Random().Next(10000, 99999).ToString();
                var modeloUsuario = new Usuario
                {
                    NombreUsuario = request.Nombre,
                    Correo = request.Correo,
                    ContraseñaHash = util.encriptarSHA256(request.Clave),
                    CodigoVerificacion = codigoVerificacion,
                    FechaExpiracionCodigo = DateTime.Now.AddHours(1),
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
                    return Ok(new RegistrarseResponse { IsSuccess = true, Message = "Registro exitoso. Verifica tu correo electrónico." });
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new RegistrarseResponse { IsSuccess = false, Message = "Error al enviar el correo. Inténtalo de nuevo." });

            }
            catch (DbUpdateException dbEx)
            {
                var sqlException = dbEx.InnerException as SqlException;
                if (sqlException != null)


                    if (sqlException.Number == 2627)
                    {

                        if (sqlException.Message.Contains("UQ__Usuarios__60695A1984046E71"))
                            return StatusCode(StatusCodes.Status409Conflict, new
                            {
                                IsSuccess = false,
                                Message = "El correo electrónico ya está en uso.",
                                Details = sqlException.Message
                            });
                    }

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    IsSuccess = false,
                    Message = "Error al guardar los cambios en la base de datos.",
                    Details = dbEx.Message
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
                else
                    return Ok(new LoginResponse { IsSuccess = true, Message = "Inicio de sesión exitoso." });






                /*
                 Omision de Codigo para futura implementacion
                 */

                //var token = util.generarJWT(usuarioEncontrado);
                //var emailBody = $@"
                //     <html>
                //     <body>
                //    <h2>Token de Inicio de Sesión</h2>
                //    <p>Tu token JWT es:
                //    <br>
                //    <strong>{token}</strong></p>
                //    </body>
                //    </html>
                //     ";

                //var emailSent = await emailService.SendEmailAsync(
                //    "Remitente",
                //    "alenaguilar24@gmail.com",
                //    "Destinatario",
                //    loginRequest.Correo,
                //    "Token de Inicio de Sesión",
                //    emailBody);

                //if (emailSent)
                //{
                //    return Ok(new LoginResponse { IsSuccess = true, Token = token, Message = "Envío exitoso. Verifica tu correo electrónico." });
                //}
                //else
                //{
                //    return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponse { IsSuccess = false, Message = "Error al enviar el correo. Inténtalo de nuevo." });
                //}
            }
            catch (Exception ex)
            {
                



                return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponse { IsSuccess = false, Message = "Error interno del servidor." });
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




    }
}

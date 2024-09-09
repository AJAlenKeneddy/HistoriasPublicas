using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StoryBlazeServer.Models;
using StoryBlazeServer.Services;

namespace StoryBlazeServer.Controllers
{
    [Route("api/[controller]")]
    
    [ApiController]
    public class HistoriasController : ControllerBase
    {
        private readonly StoryBlazeServerContext _context;
        private readonly IJwtService _jwtService;

        public HistoriasController(StoryBlazeServerContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }
        [HttpGet("ListarHistorias")]
        public async Task<IActionResult> ListarHistorias()
        {
            try
            {
                var historias = await _context.Historias
                    .Where(h => !h.Eliminado)
                    .ToListAsync();

                if (!historias.Any())
                    return NotFound(new { IsSuccess = false, Message = "No se encontraron historias." });

                return Ok(new { IsSuccess = true, Data = historias });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error interno del servidor.", Details = ex.Message });
            }
        }
        [HttpGet("ObtenerHistoria/{id}")]
        public async Task<IActionResult> ObtenerHistoria(int id)
        {
            try
            {
                var historia = await _context.Historias
                    .Where(h => h.HistoriaId == id && !h.Eliminado)
                    .FirstOrDefaultAsync();

                if (historia == null)
                    return NotFound(new { IsSuccess = false, Message = "Historia no encontrada." });

                return Ok(new { IsSuccess = true, Data = historia });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al obtener la historia.", Details = ex.Message });
            }
        }

       

        [HttpPost("AgregarHistoria")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AgregarHistoria([FromBody] Historia nuevaHistoria)
        {
            if (nuevaHistoria == null)
                return BadRequest(new { IsSuccess = false, Message = "Datos inválidos." });

            try
            {
                _context.Historias.Add(nuevaHistoria);
                await _context.SaveChangesAsync();
                return Ok(new { IsSuccess = true, Message = "Historia agregada exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al agregar la historia", Details = ex.Message });
            }
        }

        

        

        [HttpPut("ActualizarHistoria/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ActualizarHistoria(int id, [FromBody] Historia historiaActualizada)
        {
            if (historiaActualizada == null || id != historiaActualizada.HistoriaId)
                return BadRequest(new { IsSuccess = false, Message = "Datos inválidos." });

            try
            {
                var historiaExistente = await _context.Historias.FindAsync(id);

                if (historiaExistente == null || historiaExistente.Eliminado)
                    return NotFound(new { IsSuccess = false, Message = "Historia no encontrada o eliminada." });

                // Actualiza las propiedades necesarias
                historiaExistente.Titulo = historiaActualizada.Titulo;
                historiaExistente.Resumen = historiaActualizada.Resumen;

                await _context.SaveChangesAsync();
                return Ok(new { IsSuccess = true, Message = "Historia actualizada exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    IsSuccess = false,
                    Message = "Error al actualizar la historia.",
                    Details = ex.Message
                });
            }
        }

        [HttpDelete("EliminarHistoria/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> EliminarHistoria(int id)
        {
            try
            {
                var historiaExistente = await _context.Historias.FindAsync(id);

                if (historiaExistente == null)
                    return NotFound(new { IsSuccess = false, Message = "Historia no encontrada." });

                historiaExistente.Eliminado = true;
                await _context.SaveChangesAsync();

                return Ok(new { IsSuccess = true, Message = "Historia eliminada lógicamente exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al eliminar la historia.", Details = ex.Message });
            }
        }

        

        [HttpPut("RestaurarHistoria/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> RestaurarHistoria(int id)
        {
            try
            {
                var historiaExistente = await _context.Historias
                    .Where(h => h.HistoriaId == id && h.Eliminado)
                    .FirstOrDefaultAsync();

                if (historiaExistente == null)
                    return NotFound(new { IsSuccess = false, Message = "Historia no encontrada o no está eliminada." });

                historiaExistente.Eliminado = false;
                await _context.SaveChangesAsync();

                return Ok(new { IsSuccess = true, Message = "Historia restaurada exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al restaurar la historia.", Details = ex.Message });
            }
        }

        [HttpGet("usuario/historias")]
        public async Task<ActionResult<Response<List<Historia>>>> GetHistoriasByUser()
        {
            try
            {
                // Obtén el token desde el encabezado de la solicitud
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Obtén el UserId desde el token
                var userIdString = _jwtService.GetUserIdFromToken(token);

                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized(new Response<List<Historia>> { IsSuccess = false, Message = "Token inválido o no se pudo obtener el ID de usuario." });
                }

                // Convertir el userId a entero
                if (!int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized(new Response<List<Historia>> { IsSuccess = false, Message = "El ID de usuario no es válido." });
                }

                // Buscar historias del usuario
                var historias = await _context.Historias
                                              .Where(h => h.UsuarioCreadorId == userId)
                                              .ToListAsync();

                // Envolver las historias en el objeto Response<>
                var response = new Response<List<Historia>>
                {
                    IsSuccess = true,
                    Data = historias
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response<List<Historia>> { IsSuccess = false, Message = $"Error interno del servidor: {ex.Message}" });
            }
        }



    }





}

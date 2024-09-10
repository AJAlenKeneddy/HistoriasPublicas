﻿using System;
using System.Collections.Generic;
using System.Data;
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
        private readonly StoryBlazeContext _context;
        private readonly IJwtService _jwtService;

        public HistoriasController(StoryBlazeContext context, IJwtService jwtService)
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

        [HttpGet("ObtenerHistoriaCompleta/{historiaId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Historia>> ObtenerHistoriaCompleta(int historiaId)
        {
            try
            {
                // Obtener la historia, fragmentos y comentarios mediante el SP
                var historiaCompleta = new Historia();

                // Ejecutar el procedimiento almacenado y mapear los resultados
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "ObtenerHistoriaCompleta";
                    command.CommandType = CommandType.StoredProcedure;

                    // Añadir parámetro
                    var param = command.CreateParameter();
                    param.ParameterName = "@HistoriaId";
                    param.Value = historiaId;
                    command.Parameters.Add(param);

                    await _context.Database.OpenConnectionAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Leer la historia
                        if (await reader.ReadAsync())
                        {
                            historiaCompleta.HistoriaId = reader.GetInt32(0);
                            historiaCompleta.Titulo = reader.GetString(1);
                            historiaCompleta.Resumen = reader.GetString(2);
                            historiaCompleta.FechaCreacion = reader.GetDateTime(3);
                            historiaCompleta.Estado = reader.GetString(4);
                            historiaCompleta.UsuarioCreadorId = reader.GetInt32(5);
                        }

                        // Leer los fragmentos
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var fragmento = new Fragmento
                                {
                                    FragmentoId = reader.GetInt32(0),
                                    Contenido = reader.GetString(1),
                                    FechaCreacionFrag = reader.GetDateTime(2),
                                    UsuarioId = reader.GetInt32(4),
                                    TotalVotos = reader.GetInt32(5)

                                };

                                historiaCompleta.Fragmentos.Add(fragmento);
                            }
                        }

                        // Leer los comentarios
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var comentario = new Comentario
                                {
                                    ComentarioId = reader.GetInt32(0),
                                    Comentario1 = reader.GetString(1),
                                    FechaComentario = reader.GetDateTime(2),
                                    UsuarioId = reader.GetInt32(4)
                                };

                                // Asignar el comentario al fragmento correspondiente
                                var fragmento = historiaCompleta.Fragmentos.FirstOrDefault(f => f.FragmentoId == reader.GetInt32(3));
                                if (fragmento != null)
                                {
                                    fragmento.Comentarios.Add(comentario);
                                }
                            }
                        }
                    }
                }

                if (historiaCompleta == null)
                {
                    return NotFound();
                }

                return Ok(historiaCompleta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocurrió un error: {ex.Message}");
            }



        }
    }





}

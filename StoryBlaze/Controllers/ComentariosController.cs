using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryBlaze.Models;

namespace StoryBlaze.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComentariosController : ControllerBase
    {
        private readonly StoryBlazeContext _context;

        public ComentariosController(StoryBlazeContext context)
        {
            _context = context;
        }

        // GET: api/Comentarios
        // Obtiene todos los comentarios no eliminados lógicamente en la base de datos.
        [HttpGet("ObtenerComentarios")]
        public async Task<IActionResult> GetComentarios()
        {
            try
            {
                var comentarios = await _context.Comentarios
                    .Where(c => !c.Eliminado) // Filtra los comentarios no eliminados
                    .ToListAsync();

                if (comentarios == null || !comentarios.Any())
                    return NotFound(new { IsSuccess = false, Message = "No se encontraron comentarios." });

                return Ok(new { IsSuccess = true, Data = comentarios });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error interno del servidor.", Details = ex.Message });
            }
        }

        // GET: api/Comentarios/5
        // Obtiene un comentario específico por su ID si no ha sido eliminado lógicamente.
        [HttpGet("ComentariosPorID/{id}")]
        public async Task<IActionResult> GetComentario(int id)
        {
            try
            {
                var comentario = await _context.Comentarios
                    .Where(c => c.ComentarioId == id && !c.Eliminado) 
                    .FirstOrDefaultAsync();

                if (comentario == null)
                    return NotFound(new { IsSuccess = false, Message = "Comentario no encontrado." });

                return Ok(new { IsSuccess = true, Data = comentario });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error interno del servidor.", Details = ex.Message });
            }
        }

        // PUT: api/Comentarios/5
        // Actualiza un comentario existente por su ID si no ha sido eliminado lógicamente.
        [HttpPut("EditarComentario/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PutComentario(int id, [FromBody] Comentario comentario)
        {
            if (id != comentario.ComentarioId)
                return BadRequest(new { IsSuccess = false, Message = "ID de comentario no coincide." });

            // Verifica si el comentario existe y no está eliminado
            var comentarioExistente = await _context.Comentarios
                .Where(c => c.ComentarioId == id && !c.Eliminado)
                .FirstOrDefaultAsync();

            if (comentarioExistente == null)
                return NotFound(new { IsSuccess = false, Message = "Comentario no encontrado." });

            _context.Entry(comentarioExistente).CurrentValues.SetValues(comentario);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { IsSuccess = true, Message = "Comentario actualizado exitosamente." });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComentarioExists(id))
                    return NotFound(new { IsSuccess = false, Message = "Comentario no encontrado." });
                else
                    throw;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al actualizar el comentario.", Details = ex.Message });
            }
        }

        // POST: api/Comentarios
        // Crea un nuevo comentario en la base de datos.
        [HttpPost("CrearComentario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PostComentario([FromBody] Comentario comentario)
        {
            try
            {
                if (comentario == null)
                    return BadRequest(new { IsSuccess = false, Message = "Datos inválidos." });

                _context.Comentarios.Add(comentario);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetComentario), new { id = comentario.ComentarioId }, comentario);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al agregar el comentario.", Details = ex.Message });
            }
        }

        // DELETE: api/Comentarios/5
        // Marca un comentario como eliminado lógicamente por su ID.
        [HttpDelete("EliminarComentario/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteComentario(int id)
        {
            try
            {
                var comentario = await _context.Comentarios.FindAsync(id);
                if (comentario == null)
                    return NotFound(new { IsSuccess = false, Message = "Comentario no encontrado." });

                // Marca el comentario como eliminado en lugar de eliminarlo físicamente
                comentario.Eliminado = true;
                await _context.SaveChangesAsync();

                return Ok(new { IsSuccess = true, Message = "Comentario eliminado ." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al eliminar el comentario.", Details = ex.Message });
            }
        }
        // PUT: api/Comentarios/RestaurarComentario/5
        // Restaura un comentario que ha sido eliminado lógicamente por su ID.
        [HttpPut("RestaurarComentario/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> RestaurarComentario(int id)
        {
            try
            {
                // Busca el comentario por ID que ha sido eliminado lógicamente
                var comentarioExistente = await _context.Comentarios
                    .Where(c => c.ComentarioId == id && c.Eliminado) // Filtra por ID y si está eliminado
                    .FirstOrDefaultAsync();

                if (comentarioExistente == null)
                    return NotFound(new { IsSuccess = false, Message = "Comentario no encontrado o no está eliminado." });

                // Restaura el comentario (marca como no eliminado)
                comentarioExistente.Eliminado = false;
                await _context.SaveChangesAsync();

                return Ok(new { IsSuccess = true, Message = "Comentario restaurado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al restaurar el comentario.", Details = ex.Message });
            }
        }



        private bool ComentarioExists(int id)
        {
            return _context.Comentarios.Any(e => e.ComentarioId == id);
        }
    }
}

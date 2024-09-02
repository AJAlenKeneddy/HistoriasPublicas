using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StoryBlaze.Models;

namespace StoryBlaze.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoriasController : ControllerBase
    {
        private readonly StoryBlazeContext _context;

        public HistoriasController(StoryBlazeContext context)
        {
            _context = context;
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
    }



}

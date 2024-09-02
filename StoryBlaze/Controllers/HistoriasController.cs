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
        private readonly StoryBlazeContext db;

        public HistoriasController(StoryBlazeContext db)
        {
            this.db = db;
        }

        [HttpPost]
        [Route("AgregarHistoria")]
        public async Task<IActionResult> AgregarHistoria([FromBody] Historia nuevaHistoria)
        {
            try
            {
                if (nuevaHistoria == null)
                    return BadRequest(new { IsSuccess = false, Message = "Datos inválidos." });

                db.Historias.Add(nuevaHistoria);
                await db.SaveChangesAsync();

                return Ok(new { IsSuccess = true, Message = "Historia agregada exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al agregar la historia", Details = ex.Message });
            }
        }

        [HttpGet]
        [Route("ListarHistorias")]
        public async Task<IActionResult> ListarHistorias()
        {
            try
            {
                var historias = await db.Historias
                    .Where(h => !h.Eliminado)
                    .ToListAsync();

                if (historias == null || !historias.Any())
                    return NotFound(new { IsSuccess = false, Message = "No se encontraron historias." });

                return Ok(new { IsSuccess = true, Data = historias });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error interno del servidor.", Details = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerHistoria/{id}")]
        public async Task<IActionResult> ObtenerHistoria(int id)
        {
            try
            {
                var historia = await db.Historias
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

        [HttpPut]
        [Route("ActualizarHistoria/{id}")]
        public async Task<IActionResult> ActualizarHistoria(int id, [FromBody] Historia historiaActualizada)
        {
            try
            {
                var historiaExistente = await db.Historias.FindAsync(id);

                if (historiaExistente == null || historiaExistente.Eliminado)
                    return NotFound(new { IsSuccess = false, Message = "No se encontró la historia." });

                historiaExistente.Titulo = historiaActualizada.Titulo;
                historiaExistente.Resumen = historiaActualizada.Resumen;

                await db.SaveChangesAsync();

                return Ok(new { IsSuccess = true, Message = "Historia actualizada exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al actualizar la historia.", Details = ex.Message });
            }
        }

        [HttpDelete]
        [Route("EliminarHistoria/{id}")]
        public async Task<IActionResult> EliminarHistoria(int id)
        {
            try
            {
                var historiaExistente = await db.Historias.FindAsync(id);

                if (historiaExistente == null)
                    return NotFound(new { IsSuccess = false, Message = "Historia no encontrada." });

                historiaExistente.Eliminado = true;

                await db.SaveChangesAsync();

                return Ok(new { IsSuccess = true, Message = "Historia eliminada lógicamente exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al eliminar la historia.", Details = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/fragmentos/{id}")]
        public async Task<IActionResult> ObtenerFragmentosPorHistoria(int id)
        {
            try
            {
                var fragmentos = await db.Sp_ListarFragmentosPorHistorias
                    .FromSqlRaw("EXEC sp_ListarFragmentosPorHistoria @HistoriaID", new SqlParameter("@HistoriaID", id))
                    .ToListAsync();

                if (fragmentos == null || !fragmentos.Any())
                    return NotFound(new { IsSuccess = false, Message = "No se encontraron fragmentos para esta historia." });

                return Ok(new { IsSuccess = true, Data = fragmentos });
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

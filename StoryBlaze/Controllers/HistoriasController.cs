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



        [HttpGet]
        [Route("ListarHistorias")]
        public async Task<IActionResult> ListarHistorias()
        {
            try
            {
                // Llamada al procedimiento almacenado
                var historias = await db.HistoriaConUsuarios.FromSqlRaw("EXEC sp_ListarHistorias").ToListAsync();

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
        [Route("api/fragmentos/{id}")]
        public async Task<IActionResult> ObtenerFragmentosPorHistoria(int id)
        {
            try
            {
                // Ejecutar el Stored Procedure y obtener los resultados
                var fragmentos = await db.Sp_ListarFragmentosPorHistorias
                    .FromSqlRaw("EXEC sp_ListarFragmentosPorHistoria @HistoriaID", new SqlParameter("@HistoriaID", id))
                    .ToListAsync();

                // Verificar si se encontraron fragmentos
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

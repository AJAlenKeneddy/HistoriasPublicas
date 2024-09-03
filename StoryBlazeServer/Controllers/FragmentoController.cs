using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StoryBlazeServer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoryBlazeServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FragmentoController : ControllerBase
    {
        private readonly StoryBlazeServerContext _context;

        public FragmentoController(StoryBlazeServerContext context)
        {
            _context = context;
        }

        // GET: api/Fragmento/ObtenerFragmentos
        // Obtiene todos los fragmentos que no han sido eliminados lógicamente.
        [HttpGet("ObtenerFragmentos")]
        public async Task<IActionResult> GetFragmentos()
        {
            try
            {
                var fragmentos = await _context.Fragmentos
                    .Where(f => !f.Eliminado) 
                    .ToListAsync();

                if (fragmentos == null || !fragmentos.Any())
                    return NotFound(new { IsSuccess = false, Message = "No se encontraron fragmentos." });

                return Ok(new { IsSuccess = true, Data = fragmentos });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error interno del servidor.", Details = ex.Message });
            }
        }

        // GET: api/Fragmento/FramentoId/{id}
        // Obtiene un fragmento específico por su ID si no ha sido eliminado lógicamente.
        [HttpGet("FramentoId/{id}")]
        public async Task<IActionResult> GetFragmento(int id)
        {
            try
            {
                var fragmento = await _context.Fragmentos
                    .Where(f => f.FragmentoId == id && !f.Eliminado) 
                    .FirstOrDefaultAsync();

                if (fragmento == null)
                    return NotFound(new { IsSuccess = false, Message = "Fragmento no encontrado." });

                return Ok(new { IsSuccess = true, Data = fragmento });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error interno del servidor.", Details = ex.Message });
            }
        }

        // GET: api/Fragmento/ObtenerFragmentosPorHistoria/{id}
        // Obtiene los fragmentos asociados a una historia específica utilizando un stored procedure.
        [HttpGet("ObtenerFragmentosPorHistoria/{id}")]
        public async Task<IActionResult> ObtenerFragmentosPorHistoria(int id)
        {
            try
            {
                var fragmentos = await _context.Sp_ListarFragmentosPorHistorias
                    .FromSqlRaw("EXEC sp_ListarFragmentosPorHistoria @HistoriaID", new SqlParameter("@HistoriaID", id))
                    .ToListAsync();

                if (!fragmentos.Any())
                    return NotFound(new { IsSuccess = false, Message = "No se encontraron fragmentos para esta historia." });

                return Ok(new { IsSuccess = true, Data = fragmentos });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error interno del servidor.", Details = ex.Message });
            }
        }

        // PUT: api/Fragmento/AgregarFragmento/{id}
        // Actualiza un fragmento existente por su ID. Solo los fragmentos no eliminados son actualizables.
        [HttpPut("AgregarFragmento/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PutFragmento(int id, [FromBody] Fragmento fragmentoActualizado)
        {
            if (id != fragmentoActualizado.FragmentoId)
                return BadRequest(new { IsSuccess = false, Message = "ID de fragmento no coincide." });

            var fragmentoExistente = await _context.Fragmentos
                .Where(f => f.FragmentoId == id && !f.Eliminado) 
                .FirstOrDefaultAsync();

            if (fragmentoExistente == null)
                return NotFound(new { IsSuccess = false, Message = "Fragmento no encontrado." });

            
            fragmentoExistente.Contenido = fragmentoActualizado.Contenido;


            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { IsSuccess = true, Message = "Fragmento actualizado exitosamente." });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FragmentoExists(id))
                    return NotFound(new { IsSuccess = false, Message = "Fragmento no encontrado." });
                else
                    throw;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al actualizar el fragmento.", Details = ex.Message });
            }
        }

        // POST: api/Fragmento/ActualizarFragmento
        // Agrega un nuevo fragmento a la base de datos.
        [HttpPost("ActualizarFragmento")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PostFragmento([FromBody] Fragmento nuevoFragmento)
        {
            try
            {
                if (nuevoFragmento == null)
                    return BadRequest(new { IsSuccess = false, Message = "Datos inválidos." });

                _context.Fragmentos.Add(nuevoFragmento);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetFragmento), new { id = nuevoFragmento.FragmentoId }, nuevoFragmento);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al agregar el fragmento.", Details = ex.Message });
            }
        }

        // DELETE: api/Fragmento/{id}
        // Elimina lógicamente un fragmento por su ID, marcándolo como eliminado en lugar de eliminarlo físicamente.
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteFragmento(int id)
        {
            try
            {
                var fragmentoExistente = await _context.Fragmentos
                    .Where(f => f.FragmentoId == id && !f.Eliminado)
                    .FirstOrDefaultAsync();

                if (fragmentoExistente == null)
                    return NotFound(new { IsSuccess = false, Message = "Fragmento no encontrado." });

                
                fragmentoExistente.Eliminado = true;
                await _context.SaveChangesAsync();

                return Ok(new { IsSuccess = true, Message = "Fragmento eliminado lógicamente exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al eliminar el fragmento.", Details = ex.Message });
            }
        }

        // PUT: api/Fragmento/RestaurarFragmento/{id}
        // Restaura un fragmento que ha sido eliminado lógicamente, marcándolo como no eliminado.
        [HttpPut("RestaurarFragmento/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> RestaurarFragmento(int id)
        {
            try
            {
               
                var fragmentoExistente = await _context.Fragmentos
                    .Where(f => f.FragmentoId == id && f.Eliminado) 
                    .FirstOrDefaultAsync();

                if (fragmentoExistente == null)
                    return NotFound(new { IsSuccess = false, Message = "Fragmento no encontrado o no está eliminado." });

               
                fragmentoExistente.Eliminado = false;
                await _context.SaveChangesAsync();

                return Ok(new { IsSuccess = true, Message = "Fragmento restaurado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al restaurar el fragmento.", Details = ex.Message });
            }
        }

        private bool FragmentoExists(int id)
        {
            return _context.Fragmentos.Any(e => e.FragmentoId == id && !e.Eliminado);
        }
    }
}

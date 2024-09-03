using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryBlazeServer.Models;

namespace StoryBlazeServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotosController : ControllerBase
    {
        private readonly StoryBlazeServerContext _context;

        public VotosController(StoryBlazeServerContext context)
        {
            _context = context;
        }

        // POST: api/Votos/Votar
        // Permite a un usuario votar en un fragmento.
        [HttpPost("Votar")]
        public async Task<IActionResult> Votar([FromBody] Voto voto)
        {
            try
            {
                // Verifica si ya existe un voto del usuario en este fragmento
                var votoExistente = await _context.Votos
                    .FirstOrDefaultAsync(v => v.FragmentoId == voto.FragmentoId && v.UsuarioId == voto.UsuarioId);

                if (votoExistente != null)
                {
                    return BadRequest(new { IsSuccess = false, Message = "El usuario ya ha votado este fragmento." });
                }

                voto.FechaVoto = DateTime.Now;
                _context.Votos.Add(voto);
                await _context.SaveChangesAsync();

                return Ok(new { IsSuccess = true, Message = "Voto registrado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al registrar el voto.", Details = ex.Message });
            }
        }

        // DELETE: api/Votos/QuitarVoto/{fragmentoId}/{usuarioId}
        // Permite a un usuario quitar su voto de un fragmento.
        [HttpDelete("QuitarVoto/{fragmentoId}/{usuarioId}")]
        public async Task<IActionResult> QuitarVoto(int fragmentoId, int usuarioId)
        {
            try
            {
                var voto = await _context.Votos
                    .FirstOrDefaultAsync(v => v.FragmentoId == fragmentoId && v.UsuarioId == usuarioId);

                if (voto == null)
                {
                    return NotFound(new { IsSuccess = false, Message = "No se encontró el voto para quitar." });
                }

                _context.Votos.Remove(voto);
                await _context.SaveChangesAsync();

                return Ok(new { IsSuccess = true, Message = "Voto eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al quitar el voto.", Details = ex.Message });
            }
        }

        // GET: api/Votos/ContarVotos/{fragmentoId}
        // Obtiene el recuento de votos de un fragmento específico.
        [HttpGet("ContarVotos/{fragmentoId}")]
        public async Task<IActionResult> ContarVotos(int fragmentoId)
        {
            try
            {
                var totalVotos = await _context.Votos
                    .CountAsync(v => v.FragmentoId == fragmentoId);

                return Ok(new { IsSuccess = true, TotalVotos = totalVotos });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error al contar los votos.", Details = ex.Message });
            }
        }
    }
}

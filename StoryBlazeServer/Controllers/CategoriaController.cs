﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StoryBlazeServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StoryBlazeServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly StoryBlazeContext _context;

        public CategoriaController(StoryBlazeContext context)
        {
            _context = context;
        }

        // GET: api/Categoria/BuscarCategorias
        // Realiza una búsqueda de categorías cuyos nombres coincidan parcialmente con la cadena dada.
        [HttpGet("BuscarCategorias")]
        public async Task<IActionResult> BuscarCategorias(string nombre)
        {
            try
            {
                var categorias = await _context.Categoria
                    .FromSqlRaw("EXEC sp_BuscarCategoriasPorNombre @Nombre", new SqlParameter("@Nombre", nombre))
                    .ToListAsync();

                if (categorias == null || categorias.Count == 0)
                    return NotFound(new { IsSuccess = false, Message = "No se encontraron categorías." });

                return Ok(new { IsSuccess = true, Data = categorias });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error interno del servidor.", Details = ex.Message });
            }
        }

        [HttpGet("ListadoCategoria")]
        public async Task<IActionResult> ListarCategorias()
        {
            try
            {
                var listado = await _context.Categoria.ToListAsync();

                if (!listado.Any())
                    return NotFound(new { IsSuccess = false, Message = "No se encontraron Categorias" });

                return Ok(new { IsSuccess = true, Data = listado });


            }
            catch(Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "Error interno del servidor.", Details = ex.Message });
            }
        }
    }
}

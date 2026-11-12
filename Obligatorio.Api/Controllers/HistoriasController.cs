using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Obligatorio.LogicaAccesoDatos.Repositorios;
using Obligatorio.LogicaNegocio.Entidades;

namespace Obligatorio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Lector")]
    public class HistoriasController : ControllerBase
    {
        private RepositorioHistorias _repoHistorias = new RepositorioHistorias();
        private RepositorioCategorias _repoCategorias = new RepositorioCategorias();

        // RF07 - Catálogo público para los lectores autenticados, filtrable por categoría.
        [HttpGet]
        public IActionResult ObtenerHistorias([FromQuery] int? categoriaId)
        {
            if (categoriaId.HasValue && categoriaId.Value <= 0)
                return BadRequest("La categoría no es válida");

            List<Historia> historias = _repoHistorias.ListarPublicadas(categoriaId);
            return Ok(historias.Select(h => new
            {
                h.Id, h.Titulo, h.Sinopsis,
                categorias = h.Categorias.Select(c => c.Nombre).ToList()
            }).ToList());
        }

        [HttpGet("categorias")]
        public IActionResult ObtenerCategorias()
        {
            return Ok(_repoCategorias.Listar().Select(c => new { c.Id, c.Nombre }).ToList());
        }
    }
}

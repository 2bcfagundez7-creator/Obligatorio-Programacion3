using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Obligatorio.LogicaAccesoDatos.Repositorios;

namespace Obligatorio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class EstadisticasController : ControllerBase
    {
        private RepositorioEstadisticas _repo = new RepositorioEstadisticas();

        // RF09 - Historias con lecturas completadas, ordenadas de mayor a menor.
        [HttpGet("historias")]
        public IActionResult RankingHistorias()
        {
            return Ok(_repo.RankingHistorias());
        }

        // RF10 - Listado para elegir una historia.
        [HttpGet("historias-disponibles")]
        public IActionResult HistoriasDisponibles()
        {
            return Ok(_repo.ListarHistorias());
        }

        // RF10 - Para la historia elegida mostramos todos los finales y el más frecuente.
        [HttpGet("finales/{historiaId:int}")]
        public IActionResult RankingFinales(int historiaId)
        {
            if (historiaId <= 0) return BadRequest("Seleccioná una historia");
            if (!_repo.ListarHistorias().Any(h => h.HistoriaId == historiaId))
                return NotFound("La historia no existe");

            var finales = _repo.RankingFinales(historiaId);
            int maximo = finales.Count == 0 ? 0 : finales.Max(f => f.VecesAlcanzado);
            return Ok(new
            {
                historiaId,
                finales,
                // Sin lecturas no declaramos un final ganador.
                masFrecuentes = maximo == 0 ? new List<int>() :
                    finales.Where(f => f.VecesAlcanzado == maximo)
                        .Select(f => f.CapituloId).ToList()
            });
        }

        // RF11 - Lectores con más historias distintas completadas.
        [HttpGet("lectores")]
        public IActionResult RankingLectores()
        {
            return Ok(_repo.RankingLectores());
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Obligatorio.WebLectores.Models;
using Obligatorio.WebLectores.Servicios;
using System.Net;

namespace Obligatorio.WebLectores.Controllers
{
    public class RankingsController : Controller
    {
        private readonly ServicioApi _api;

        public RankingsController(ServicioApi api)
        {
            _api = api;
        }

        // RF09, RF10 y RF11 - Solamente el administrador consulta las métricas.
        public async Task<IActionResult> Index(int? historiaId)
        {
            string? token = HttpContext.Session.GetString("Token");
            if (token == null) return RedirectToAction("Login", "Login");
            if (HttpContext.Session.GetString("Rol") != "Administrador") return Forbid();

            ResultadoApi<List<RankingHistoriaDTO>> historias =
                await _api.GetAsync<List<RankingHistoriaDTO>>("api/Estadisticas/historias", token);
            ResultadoApi<List<RankingHistoriaDTO>> disponibles =
                await _api.GetAsync<List<RankingHistoriaDTO>>("api/Estadisticas/historias-disponibles", token);
            ResultadoApi<List<RankingLectorDTO>> lectores =
                await _api.GetAsync<List<RankingLectorDTO>>("api/Estadisticas/lectores", token);

            if (historias.Codigo == HttpStatusCode.Unauthorized ||
                disponibles.Codigo == HttpStatusCode.Unauthorized ||
                lectores.Codigo == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Login");
            }

            RankingsDTO modelo = new RankingsDTO();
            modelo.Historias = historias.Datos ?? new List<RankingHistoriaDTO>();
            modelo.OpcionesHistorias = disponibles.Datos ?? new List<RankingHistoriaDTO>();
            modelo.Lectores = lectores.Datos ?? new List<RankingLectorDTO>();
            modelo.HistoriaSeleccionadaId = historiaId;

            if (modelo.HistoriaSeleccionadaId == null && modelo.OpcionesHistorias.Count > 0)
                modelo.HistoriaSeleccionadaId = modelo.OpcionesHistorias[0].HistoriaId;

            if (modelo.HistoriaSeleccionadaId != null)
            {
                ResultadoApi<FinalesPorHistoriaDTO> finales =
                    await _api.GetAsync<FinalesPorHistoriaDTO>(
                        "api/Estadisticas/finales/" + modelo.HistoriaSeleccionadaId, token);

                if (finales.Codigo == HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "Login");
                }
                modelo.Finales = finales.Datos?.Finales ?? new List<RankingFinalDTO>();
                modelo.FinalesMasFrecuentes = finales.Datos?.MasFrecuentes ?? new List<int>();
                if (!finales.Correcto) modelo.Error = finales.Error;
            }

            if (!historias.Correcto || !disponibles.Correcto || !lectores.Correcto)
                modelo.Error = "No se pudieron cargar todas las métricas. Revisá la conexión con la API";

            return View(modelo);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Obligatorio.WebLectores.Models;
using Obligatorio.WebLectores.Servicios;
using System.Net;

namespace Obligatorio.WebLectores.Controllers
{
    public class HistoriasController : Controller
    {
        private readonly ServicioApi _api;

        public HistoriasController(ServicioApi api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(int? categoriaId)
        {
            string? token = HttpContext.Session.GetString("Token");
            if (token == null) return RedirectToAction("Login", "Login");
            if (HttpContext.Session.GetString("Rol") != "Lector")
                return Forbid();

            // RF07 - Obtenemos el catálogo y sus categorías desde la API.
            ResultadoApi<List<CategoriaDTO>> categorias =
                await _api.GetAsync<List<CategoriaDTO>>("api/Historias/categorias", token);
            string ruta = "api/Historias";
            if (categoriaId.HasValue) ruta += "?categoriaId=" + categoriaId.Value;

            ResultadoApi<List<HistoriaPublicadaDTO>> resultado =
                await _api.GetAsync<List<HistoriaPublicadaDTO>>(ruta, token);

            if (resultado.Codigo == HttpStatusCode.Unauthorized ||
                categorias.Codigo == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Login");
            }

            ViewBag.Categorias = categorias.Datos ?? new List<CategoriaDTO>();
            ViewBag.CategoriaId = categoriaId;
            if (!resultado.Correcto || !categorias.Correcto)
                ViewBag.Error = resultado.Error != "" ? resultado.Error : categorias.Error;

            return View(resultado.Datos ?? new List<HistoriaPublicadaDTO>());
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Obligatorio.WebLectores.Models;
using Obligatorio.WebLectores.Servicios;
using System.Net;

namespace Obligatorio.WebLectores.Controllers
{
    public class HistorialController : Controller
    {
        private readonly ServicioApi _api;

        public HistorialController(ServicioApi api)
        {
            _api = api;
        }

        // RF08 - Historial del mes y año que elige el lector.
        public async Task<IActionResult> Index(int? mes, int? anio)
        {
            string? token = HttpContext.Session.GetString("Token");
            if (token == null) return RedirectToAction("Login", "Login");
            if (HttpContext.Session.GetString("Rol") != "Lector") return Forbid();

            int mesElegido = mes ?? DateTime.Now.Month;
            int anioElegido = anio ?? DateTime.Now.Year;
            if (mesElegido < 1 || mesElegido > 12 || anioElegido < 1900 || anioElegido > 2100)
            {
                ViewBag.Error = "Ingresá un mes y año válidos";
                mesElegido = DateTime.Now.Month;
                anioElegido = DateTime.Now.Year;
            }

            ViewBag.Mes = mesElegido;
            ViewBag.Anio = anioElegido;

            ResultadoApi<List<LecturaHistorialDTO>> resultado =
                await _api.GetAsync<List<LecturaHistorialDTO>>(
                    "api/Lecturas/historial?mes=" + mesElegido + "&anio=" + anioElegido, token);

            if (resultado.Codigo == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Login");
            }

            if (!resultado.Correcto) ViewBag.Error = resultado.Error;
            return View(resultado.Datos ?? new List<LecturaHistorialDTO>());
        }
    }
}

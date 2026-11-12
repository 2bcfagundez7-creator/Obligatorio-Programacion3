using Microsoft.AspNetCore.Mvc;
using Obligatorio.WebLectores.Models;
using Obligatorio.WebLectores.Servicios;
using System.Net;

namespace Obligatorio.WebLectores.Controllers
{
    public class LecturasController : Controller
    {
        private readonly ServicioApi _api;

        public LecturasController(ServicioApi api)
        {
            _api = api;
        }

        // RF07 - Iniciamos una lectura desde el listado de historias
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Iniciar(int historiaId)
        {
            string? token = HttpContext.Session.GetString("Token");
            if (token == null) return RedirectToAction("Login", "Login");
            if (HttpContext.Session.GetString("Rol") != "Lector") return Forbid();

            ResultadoApi<LecturaIniciadaDTO> resultado =
                await _api.PostAsync<LecturaIniciadaDTO>(
                    "api/Lecturas/iniciar/" + historiaId, token);

            if (resultado.Codigo == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Login");
            }

            if (!resultado.Correcto || resultado.Datos == null)
            {
                TempData["Error"] = resultado.Error;
                return RedirectToAction("Index", "Historias");
            }

            return RedirectToAction("Leer", new { lecturaId = resultado.Datos.LecturaId });
        }

        // RF07 - Mostramos el capítulo actual con sus opciones
        [HttpGet]
        public async Task<IActionResult> Leer(int lecturaId)
        {
            string? token = HttpContext.Session.GetString("Token");
            if (token == null) return RedirectToAction("Login", "Login");
            if (HttpContext.Session.GetString("Rol") != "Lector") return Forbid();

            ResultadoApi<CapituloActualDTO> resultado =
                await _api.GetAsync<CapituloActualDTO>(
                    "api/Lecturas/" + lecturaId + "/actual", token);

            if (resultado.Codigo == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Login");
            }

            if (!resultado.Correcto || resultado.Datos == null)
            {
                TempData["Error"] = resultado.Error;
                return RedirectToAction("Index", "Historial");
            }

            ViewBag.LecturaId = lecturaId;
            return View(resultado.Datos);
        }

        // RF07 - El lector elige una de las opciones mostradas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Elegir(int lecturaId, int opcionId)
        {
            string? token = HttpContext.Session.GetString("Token");
            if (token == null) return RedirectToAction("Login", "Login");
            if (HttpContext.Session.GetString("Rol") != "Lector") return Forbid();

            ResultadoApi<CapituloActualDTO> resultado =
                await _api.PostAsync<CapituloActualDTO>(
                    "api/Lecturas/" + lecturaId + "/elegir/" + opcionId, token);

            if (resultado.Codigo == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Login");
            }

            if (!resultado.Correcto)
            {
                TempData["Error"] = resultado.Error;
            }

            return RedirectToAction("Leer", new { lecturaId = lecturaId });
        }
    }
}

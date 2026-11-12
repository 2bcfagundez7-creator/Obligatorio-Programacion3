using Microsoft.AspNetCore.Mvc;
using Obligatorio.WebLectores.Models;

namespace Obligatorio.WebLectores.Controllers
{
    public class PreferenciasController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Login");
            if (HttpContext.Session.GetString("Rol") != "Lector") return Forbid();

            PreferenciaDTO modelo = new PreferenciaDTO();
            modelo.Estilo = HttpContext.Session.GetString("EstiloLectura") ?? "claro";
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(PreferenciaDTO modelo)
        {
            if (HttpContext.Session.GetString("Token") == null)
                return RedirectToAction("Login", "Login");
            if (HttpContext.Session.GetString("Rol") != "Lector") return Forbid();

            if (modelo.Estilo != "claro" && modelo.Estilo != "oscuro")
            {
                ViewBag.Error = "Seleccioná un estilo válido";
                return View(modelo);
            }

            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Login");

            // RF12 - Una cookie diferente para cada usuario en este navegador.
            CookieOptions opciones = new CookieOptions();
            opciones.HttpOnly = true;
            opciones.IsEssential = true;
            opciones.SameSite = SameSiteMode.Lax;
            opciones.Secure = Request.IsHttps;
            opciones.Expires = DateTimeOffset.UtcNow.AddMonths(6);
            Response.Cookies.Append("EstiloLectura_" + usuarioId.Value, modelo.Estilo, opciones);
            HttpContext.Session.SetString("EstiloLectura", modelo.Estilo);

            TempData["Mensaje"] = "Preferencia guardada";
            return RedirectToAction("Index");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Obligatorio.Web.Models;
using System.Diagnostics;

namespace Obligatorio.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            string? nombre = HttpContext.Session.GetString("UsuarioNombre");
            ViewBag.UsuarioNombre = nombre;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

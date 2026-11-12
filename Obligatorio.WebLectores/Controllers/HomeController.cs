using Microsoft.AspNetCore.Mvc;
using Obligatorio.WebLectores.Models;
using System.Diagnostics;

namespace Obligatorio.WebLectores.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Token") != null)
            {
                if (HttpContext.Session.GetString("Rol") == "Administrador")
                    return RedirectToAction("Index", "Rankings");
                return RedirectToAction("Index", "Historias");
            }
            return RedirectToAction("Login", "Login");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

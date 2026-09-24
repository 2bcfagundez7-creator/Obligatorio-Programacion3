using Microsoft.AspNetCore.Mvc;
using Obligatorio.LogicaAccesoDatos.Repositorios;

namespace Obligatorio.Web.Controllers
{
    public class LoginController : Controller
    {
        private RepositorioUsuarios _repoUsuarios = new RepositorioUsuarios();

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            
            var admin = _repoUsuarios.LoginAdministrador(username, password);

            if (admin != null)
            {
                HttpContext.Session.SetInt32("UsuarioId", admin.Id);
                HttpContext.Session.SetString("UsuarioNombre", admin.NombreCompleto);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Usuario o contraseña incorrectos";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}
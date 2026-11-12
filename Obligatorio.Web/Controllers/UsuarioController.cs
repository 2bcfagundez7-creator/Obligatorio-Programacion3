using Microsoft.AspNetCore.Mvc;
using Obligatorio.LogicaAccesoDatos.Repositorios;
using Obligatorio.LogicaNegocio.Entidades;

namespace Obligatorio.Web.Controllers
{
    public class UsuarioController : Controller
    {
        private RepositorioUsuarios _repoUsuarios = new RepositorioUsuarios();

        public IActionResult Crear()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(
            string nombreCompleto,
            string email,
            string username,
            string password,
            string rol)
        {
            if (HttpContext.Session.GetInt32("UsuarioId") == null)
                return RedirectToAction("Login", "Login");

            // Validamos que no haya campos vacíos
            if (string.IsNullOrEmpty(nombreCompleto) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(rol))
            {
                ViewBag.Error = "Todos los datos son obligatorios";
                return View();
            }

            // Validamos username repetido
            if (_repoUsuarios.ExisteUsername(username))
            {
                ViewBag.Error = "El nombre de usuario ya existe";
                return View();
            }

            // Validamos email repetido
            if (_repoUsuarios.ExisteEmail(email))
            {
                ViewBag.Error = "El email ya existe";
                return View();
            }

            Usuario nuevoUsuario;

            if (rol == "Administrador")
            {
                nuevoUsuario = new Administrador();
            }
            else if (rol == "Lector" || rol == "Usuario")
            {
                nuevoUsuario = new Lector();
            }
            else
            {
                ViewBag.Error = "El rol seleccionado no es válido";
                return View();
            }

            nuevoUsuario.NombreCompleto = nombreCompleto;
            nuevoUsuario.Email = email;
            nuevoUsuario.Username = username;
            nuevoUsuario.SetPassword(password);

            if (!nuevoUsuario.ValidarPassword())
            {
                ViewBag.Error = "La contraseña no cumple los requisitos";
                return View();
            }

            try
            {
                _repoUsuarios.Agregar(nuevoUsuario);
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                return View();
            }

            ViewBag.Mensaje = "Usuario creado correctamente";
            return View();
        }
    }
}

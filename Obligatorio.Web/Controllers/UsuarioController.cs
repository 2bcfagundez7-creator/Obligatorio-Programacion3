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
        public IActionResult Crear(
            string nombreCompleto,
            string email,
            string username,
            string password,
            string rol)
        {
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
            else
            {
                nuevoUsuario = new Lector();
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

            _repoUsuarios.Agregar(nuevoUsuario);

            ViewBag.Mensaje = "Usuario creado correctamente";
            return View();
        }
    }
}

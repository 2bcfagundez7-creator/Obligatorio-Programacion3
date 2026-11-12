using Microsoft.AspNetCore.Mvc;
using Obligatorio.WebLectores.Models;
using Obligatorio.WebLectores.Servicios;

namespace Obligatorio.WebLectores.Controllers
{
    public class LoginController : Controller
    {
        private readonly ServicioApi _api;

        public LoginController(ServicioApi api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Token") != null)
                return RedirigirSegunRol();

            return View(new LoginLectorDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginLectorDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                ViewBag.Error = "Tenés que ingresar usuario y contraseña";
                return View(dto);
            }

            // Enviamos los datos a la API; el MVC no accede a SQL Server.
            ResultadoApi<TokenRespuestaDTO> resultado =
                await _api.PostAsync<TokenRespuestaDTO>("api/Auth/login", null, dto);

            if (!resultado.Correcto || resultado.Datos == null ||
                string.IsNullOrWhiteSpace(resultado.Datos.Token))
            {
                ViewBag.Error = resultado.Error;
                return View(dto);
            }

            // RF06 - Tanto Administrador como Lector utilizan el JWT recibido.
            HttpContext.Session.Clear();
            HttpContext.Session.SetString("Token", resultado.Datos.Token);
            HttpContext.Session.SetString("Username", resultado.Datos.Username);
            HttpContext.Session.SetString("Rol", resultado.Datos.Rol);
            HttpContext.Session.SetInt32("UsuarioId", resultado.Datos.UsuarioId);

            if (resultado.Datos.Rol == "Lector")
            {
                // RF12 - La preferencia se mantiene por usuario en este navegador.
                string nombreCookie = "EstiloLectura_" + resultado.Datos.UsuarioId;
                string estilo = Request.Cookies[nombreCookie] ?? "claro";
                HttpContext.Session.SetString("EstiloLectura", estilo == "oscuro" ? "oscuro" : "claro");
            }

            return RedirigirSegunRol();
        }

        private IActionResult RedirigirSegunRol()
        {
            if (HttpContext.Session.GetString("Rol") == "Administrador")
                return RedirectToAction("Index", "Rankings");
            return RedirectToAction("Index", "Historias");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

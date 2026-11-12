
using Microsoft.AspNetCore.Mvc;
using Obligatorio.LogicaAccesoDatos.Repositorios;
using Obligatorio.LogicaNegocio.Entidades;

namespace Obligatorio.Web.Controllers
{
    public class CapituloController : Controller
    {
        private RepositorioHistorias _repoHistorias = new RepositorioHistorias();
        private RepositorioCapitulos _repoCapitulos = new RepositorioCapitulos();


        public IActionResult Crear(int? historiaId)
        {
            // Obtenemos el ID del usuario de la sesión
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            ViewBag.Historias = _repoHistorias.Listar();

            ViewBag.Capitulos = new List<CapituloIntermedio>();

            if (historiaId != null)
            {
                ViewBag.Capitulos = _repoCapitulos.ListarIntermedios(historiaId.Value);
            }

            ViewBag.HistoriaId = historiaId;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(
            int historiaId,
            int capituloAnteriorId,
            string tipo,
            string titulo,
            string texto,
            string textoOpcion)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (string.IsNullOrWhiteSpace(titulo) ||
                string.IsNullOrWhiteSpace(texto) ||
                string.IsNullOrWhiteSpace(textoOpcion))
            {
                ViewBag.Error = "Todos los campos son obligatorios";
                CargarDatosFormulario(historiaId);
                return View();
            }

            Capitulo nuevoCapitulo;

            if (tipo == "Intermedio")
            {
                nuevoCapitulo = new CapituloIntermedio();
            }
            else if (tipo == "Final")
            {
                nuevoCapitulo = new CapituloFinal();
            }
            else
            {
                ViewBag.Error = "Tipo de capítulo inválido";
                CargarDatosFormulario(historiaId);
                return View();
            }

            nuevoCapitulo.Titulo = titulo;
            nuevoCapitulo.Texto = texto;

            try
            {
                _repoCapitulos.AgregarCapituloAHistoria(
                    historiaId,
                    capituloAnteriorId,
                    nuevoCapitulo,
                    textoOpcion,
                    usuarioId.Value
                );

                return RedirectToAction("Index", "Historia");
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                CargarDatosFormulario(historiaId);
                return View();
            }
        }

        // Método auxiliar para cargar los datos necesarios en el formulario
        private void CargarDatosFormulario(int historiaId)
        {
            ViewBag.Historias = _repoHistorias.Listar();
            ViewBag.Capitulos = _repoCapitulos.ListarIntermedios(historiaId);
            ViewBag.HistoriaId = historiaId;
        }


        // RF04 - Mostramos los capítulos de una historia para poder editarlos
        public IActionResult Listar(int historiaId)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Login");

            Historia? historia = _repoHistorias.BuscarPorId(historiaId);
            if (historia == null) return RedirectToAction("Index", "Historia");

            return View(historia);
        }

        [HttpGet]
        public IActionResult Editar(int historiaId, int capituloId)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Login");

            Capitulo? capitulo = _repoCapitulos.BuscarPorHistoria(capituloId, historiaId);
            if (capitulo == null) return RedirectToAction("Listar", new { historiaId });

            ViewBag.HistoriaId = historiaId;
            return View(capitulo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int historiaId, int capituloId, string titulo, string texto)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Login");

            try
            {
                _repoCapitulos.EditarConAuditoria(
                    historiaId, capituloId, titulo, texto, usuarioId.Value);

                return RedirectToAction("Listar", new { historiaId });
            }
            catch (Exception e)
            {
                Capitulo? capitulo = _repoCapitulos.BuscarPorHistoria(capituloId, historiaId);
                if (capitulo == null) return RedirectToAction("Index", "Historia");
                ViewBag.Error = e.Message;
                ViewBag.HistoriaId = historiaId;
                return View(capitulo);
            }
        }

    }
}

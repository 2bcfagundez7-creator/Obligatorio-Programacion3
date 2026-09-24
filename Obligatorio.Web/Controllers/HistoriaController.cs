using Microsoft.AspNetCore.Mvc;
using Obligatorio.LogicaAccesoDatos.Repositorios;
using Obligatorio.LogicaNegocio.Entidades;

namespace Obligatorio.Web.Controllers
{
    public class HistoriaController : Controller
    {
        private RepositorioHistorias _repoHistorias = new RepositorioHistorias();
        private RepositorioCategorias _repoCategorias = new RepositorioCategorias();

        public IActionResult Index()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var historias = _repoHistorias.Listar();

            return View(historias);
        }
        public IActionResult Crear()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            // Chequeamos si el usuario está logueado, si no lo está lo redirigimos al login
            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Login");
            }
            // Obtenemos las categorías desde la base de datos para mostrarlas en el formulario de creación de historia
            var categorias = _repoCategorias.Listar();

            ViewBag.Categorias = categorias;

            return View();
        }
        [HttpPost]
            public IActionResult Crear(
                string titulo,
                string sinopsis,
                List<int> categoriasIds,
                string tituloCapitulo,
                string textoCapitulo)
            {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            // Validar campos obligatorios
            if (string.IsNullOrEmpty(titulo) ||
                string.IsNullOrEmpty(sinopsis) ||
                string.IsNullOrEmpty(tituloCapitulo) ||
                string.IsNullOrEmpty(textoCapitulo))
            {
                ViewBag.Error = "Todos los datos son obligatorios";
                ViewBag.Categorias = _repoCategorias.Listar();

                return View();
            }

            // Buscar la categoría seleccionada
            if (categoriasIds == null || categoriasIds.Count == 0)
            {
                ViewBag.Error = "Debe seleccionar al menos una categoría";
                ViewBag.Categorias = _repoCategorias.Listar();

                return View();
            }

            // Crear el capítulo inicial
            CapituloIntermedio capituloInicial = new CapituloIntermedio();

            capituloInicial.Titulo = tituloCapitulo;
            capituloInicial.Texto = textoCapitulo;

            // Crear la historia
            Historia historia = new Historia();

            historia.Titulo = titulo;
            historia.Sinopsis = sinopsis;
            historia.Estado = EstadoHistoria.BORRADOR;

            foreach (int categoriaId in categoriasIds)
            {
                Categoria? categoria = _repoCategorias.BuscarPorId(categoriaId);

                if (categoria != null)
                {
                    historia.Categorias.Add(categoria);
                }
            }

            // Primero guardamos la historia
            _repoHistorias.Agregar(historia);

            // Después guardamos y asociamos el capítulo inicial
            _repoHistorias.AgregarCapituloInicial(historia, capituloInicial);

            return RedirectToAction("Index");
        }
        public IActionResult Editar(int id)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            Historia? historia = _repoHistorias.BuscarPorId(id);

            if (historia == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Categorias = _repoCategorias.Listar();

            return View(historia);
        }
        [HttpPost]
        public IActionResult Editar(
            int id,
            string titulo,
            string sinopsis,
            string estado,
            List<int> categoriasIds,
            // Titulo capitulo, puede ser nulo, por eso el signo de interrogación
            string? tituloCapitulo,
            string? textoCapitulo)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            Historia? historia = _repoHistorias.BuscarConEstructura(id);

            if (historia == null)
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(titulo) ||
                string.IsNullOrEmpty(sinopsis))
            {
                ViewBag.Error = "Todos los datos son obligatorios";
                return View(historia);
            }

            historia.Titulo = titulo;
            historia.Sinopsis = sinopsis;

            // Si la historia tiene un solo capítulo y es el capítulo inicial, actualizamos su título y texto
            if (historia.Capitulos.Count == 1 && historia.CapituloInicial != null)
            {
                historia.CapituloInicial.Titulo = tituloCapitulo ?? "";
                historia.CapituloInicial.Texto = textoCapitulo ?? "";
            }

            // Validamos que se haya seleccionado al menos una categoría
            if (categoriasIds == null || categoriasIds.Count == 0)
            {
                ViewBag.Error = "Debe seleccionar al menos una categoría";
                ViewBag.Categorias = _repoCategorias.Listar();

                return View(historia);
            }

            if (estado == "PUBLICADA")
            {
                if (!historia.ValidarEstructura())
                {
                    ViewBag.Error = "La historia no tiene una estructura válida para ser publicada";
                    return View(historia);
                }

                historia.Estado = EstadoHistoria.PUBLICADA;
            }
            else
            {
                historia.Estado = EstadoHistoria.BORRADOR;
            }
            // Actualizamos las categorías de la historia
            _repoHistorias.ActualizarCategorias(historia, categoriasIds);
            // Guardamos los cambios en la historia
            _repoHistorias.Modificar();

            // Redirigimos al índice de historias
            return RedirectToAction("Index");
        }
        public IActionResult Eliminar(int id)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            Historia? historia = _repoHistorias.BuscarPorId(id);

            if (historia == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.Categorias = _repoCategorias.Listar();
            return View(historia);
        }
        [HttpPost]
        public IActionResult ConfirmarEliminar(int id)
        {
            // Verificamos si el usuario está logueado
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Login");
            }
            // Buscamos la historia por su id
            Historia? historia = _repoHistorias.BuscarPorId(id);

            if (historia == null)
            {
                return RedirectToAction("Index");
            }
            // Verificamos si la historia tiene lecturas asociadas
            if (_repoHistorias.TieneLecturas(id))
            {
                TempData["Error"] = "No se puede eliminar la historia porque tiene lecturas asociadas";

                return RedirectToAction("Index");
            }

            _repoHistorias.Eliminar(historia);

            return RedirectToAction("Index");
        }
    }
}
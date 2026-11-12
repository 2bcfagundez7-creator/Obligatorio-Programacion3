using Microsoft.AspNetCore.Mvc;
using Obligatorio.LogicaAccesoDatos.Repositorios;

namespace Obligatorio.Web.Controllers
{
    public class AuditoriaController : Controller
    {
        private RepositorioAuditorias _repoAuditorias = new RepositorioAuditorias();
        private RepositorioHistorias _repoHistorias = new RepositorioHistorias();

        public IActionResult Index(int? historiaId, int? administradorId)
        {
            if (HttpContext.Session.GetInt32("UsuarioId") == null)
            {
                return RedirectToAction("Login", "Login");
            }

            ViewBag.Historias = _repoHistorias.Listar();
            ViewBag.Administradores = _repoAuditorias.ListarAdministradores();
            ViewBag.HistoriaId = historiaId;
            ViewBag.AdministradorId = administradorId;
            return View(_repoAuditorias.Listar(historiaId, administradorId));
        }
    }
}

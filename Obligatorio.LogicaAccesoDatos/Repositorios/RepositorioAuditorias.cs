using Microsoft.EntityFrameworkCore;

using Obligatorio.LogicaAccesoDatos.EF;
using Obligatorio.LogicaNegocio.Entidades;

namespace Obligatorio.LogicaAccesoDatos.Repositorios
{
    public class RepositorioAuditorias
    {
        private ObligatorioContext _context = new ObligatorioContext();

        public void Agregar(Auditoria auditoria)
        {
            auditoria.Validar();
            _context.Auditorias.Add(auditoria);
            _context.SaveChanges();
        }

        public List<Auditoria> Listar(int? historiaId = null, int? administradorId = null)
        {
            IQueryable<Auditoria> consulta = _context.Auditorias
                .Include(a => a.Administrador)
                .Include(a => a.Historia);

            if (historiaId.HasValue)
            {
                consulta = consulta.Where(a => a.HistoriaId == historiaId.Value);
            }
            if (administradorId.HasValue)
            {
                consulta = consulta.Where(a => a.AdministradorId == administradorId.Value);
            }
            return consulta.OrderByDescending(a => a.Fecha).ThenByDescending(a => a.Id).ToList();
        }

        public List<Administrador> ListarAdministradores()
        {
            return _context.Administradores.OrderBy(a => a.Username).ToList();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Obligatorio.LogicaAccesoDatos.DTOs;
using Obligatorio.LogicaAccesoDatos.EF;
using Obligatorio.LogicaNegocio.Entidades;

namespace Obligatorio.LogicaAccesoDatos.Repositorios
{
    public class RepositorioEstadisticas
    {
        private ObligatorioContext _context = new ObligatorioContext();

        // RF09 - Solamente historias con al menos una lectura completada.
        public List<RankingHistoriaDatos> RankingHistorias()
        {
            return _context.Historias.AsNoTracking()
                .Select(h => new RankingHistoriaDatos
                {
                    HistoriaId = h.Id,
                    Titulo = h.Titulo,
                    LecturasIniciadas = _context.Lecturas.Count(l => l.HistoriaId == h.Id),
                    LecturasCompletadas = _context.Lecturas.Count(l =>
                        l.HistoriaId == h.Id && l.FechaFinalizacion != null)
                })
                .Where(h => h.LecturasCompletadas > 0)
                .OrderByDescending(h => h.LecturasCompletadas)
                .ThenBy(h => h.Titulo)
                .ToList();
        }

        // RF10 - Seleccionamos una historia, incluyendo finales que todavía tienen 0 lecturas.
        public List<RankingFinalDatos> RankingFinales(int historiaId)
        {
            return _context.CapitulosFinales.AsNoTracking()
                .Where(f => Microsoft.EntityFrameworkCore.EF.Property<int?>(f, "HistoriaId") == historiaId)
                .Select(f => new RankingFinalDatos
                {
                    CapituloId = f.Id,
                    TituloFinal = f.Titulo,
                    HistoriaId = historiaId,
                    TituloHistoria = _context.Historias.Where(h => h.Id == historiaId)
                        .Select(h => h.Titulo).FirstOrDefault() ?? "",
                    VecesAlcanzado = _context.Lecturas.Count(l =>
                        l.HistoriaId == historiaId &&
                        l.FechaFinalizacion != null &&
                        l.FinalAlcanzado != null && l.FinalAlcanzado.Id == f.Id)
                })
                .OrderByDescending(f => f.VecesAlcanzado)
                .ThenBy(f => f.TituloFinal)
                .ToList();
        }

        // Opciones del desplegable de historias del administrador.
        public List<RankingHistoriaDatos> ListarHistorias()
        {
            return _context.Historias.AsNoTracking().OrderBy(h => h.Titulo)
                .Select(h => new RankingHistoriaDatos { HistoriaId = h.Id, Titulo = h.Titulo })
                .ToList();
        }

        // RF11 - Cada historia distinta completada cuenta una sola vez por lector.
        public List<RankingLectorDatos> RankingLectores()
        {
            var resumenes = _context.Lecturas
                .Where(l => l.FechaFinalizacion != null)
                .GroupBy(l => l.UsuarioId)
                .Select(g => new
                {
                    UsuarioId = g.Key,
                    HistoriasCompletadas = g.Select(l => l.HistoriaId).Distinct().Count()
                });

            return (from lector in _context.Lectores.AsNoTracking()
                    join resumen in resumenes on lector.Id equals resumen.UsuarioId
                    orderby resumen.HistoriasCompletadas descending, lector.Username
                    select new RankingLectorDatos
                    {
                        UsuarioId = lector.Id,
                        Username = lector.Username,
                        HistoriasCompletadas = resumen.HistoriasCompletadas
                    }).ToList();
        }
    }
}

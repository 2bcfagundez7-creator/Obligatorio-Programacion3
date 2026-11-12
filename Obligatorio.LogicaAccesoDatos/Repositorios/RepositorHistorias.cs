
using Microsoft.EntityFrameworkCore;
using Obligatorio.LogicaAccesoDatos.EF;
using Obligatorio.LogicaNegocio.Entidades;

namespace Obligatorio.LogicaAccesoDatos.Repositorios
{
    public class RepositorioHistorias
    {
        private ObligatorioContext _context = new ObligatorioContext();

        // RF07 - Filtro de categoría mediante LINQ to Entities.
        public List<Historia> ListarPublicadas(int? categoriaId)
        {
            IQueryable<Historia> consulta = _context.Historias
                .AsNoTracking()
                .Include(h => h.Categorias)
                .Where(h => h.Estado == EstadoHistoria.PUBLICADA);

            if (categoriaId.HasValue)
            {
                consulta = consulta.Where(h =>
                    h.Categorias.Any(c => c.Id == categoriaId.Value));
            }

            return consulta.OrderBy(h => h.Titulo).ToList();
        }

        public List<Historia> Listar()
        {
            return _context.Historias.ToList();
        }

        public Historia? BuscarPorId(int id)
        {
            // Buscamos la historia por su ID y cargamos sus relaciones
            return _context.Historias
                .Include(h => h.Categorias)
                .Include(h => h.Capitulos)
                .Include(h => h.CapituloInicial)
                .FirstOrDefault(h => h.Id == id);
        }

        public void ActualizarCategorias(
            Historia historia,
            List<int> categoriasIds)
        {
            // Primero cargamos las categorías actuales de la historia
            _context.Entry(historia)
                .Collection(h => h.Categorias)
                .Load();

            // Eliminamos las relaciones actuales
            historia.Categorias.Clear();
            _context.SaveChanges();

            // Agregamos las nuevas categorías seleccionadas
            foreach (int categoriaId in categoriasIds)
            {
                Categoria? categoria = _context.Categorias
                    .FirstOrDefault(c => c.Id == categoriaId);

                if (categoria != null)
                {
                    historia.Categorias.Add(categoria);
                }
            }

            _context.SaveChanges();
        }

        public void Modificar()
        {
            _context.SaveChanges();
        }

        // Incluye los cambios de historia, categorías y la auditoría en una sola transacción.
        public void ModificarConAuditoria(
            Historia historia,
            List<int> categoriasIds,
            int administradorId,
            string accion)
        {
            using var transaccion = _context.Database.BeginTransaction();

            try
            {
                ActualizarCategorias(historia, categoriasIds);

                historia.Validar();
                _context.SaveChanges();

                Auditoria auditoria = new Auditoria();

                auditoria.Accion = accion;
                auditoria.Fecha = DateTime.Now;
                auditoria.AdministradorId = administradorId;
                auditoria.HistoriaId = historia.Id;

                auditoria.Validar();

                _context.Auditorias.Add(auditoria);
                _context.SaveChanges();

                transaccion.Commit();
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }
        }

        public void Agregar(Historia historia)
        {
            foreach (Categoria categoria in historia.Categorias)
            {
                _context.Categorias.Attach(categoria);
            }

            _context.Historias.Add(historia);
            _context.SaveChanges();
        }

        public void RegistrarCreacion(
            Historia historia,
            int administradorId)
        {
            Auditoria auditoria = new Auditoria();

            auditoria.Accion = "Crear historia";
            auditoria.Fecha = DateTime.Now;
            auditoria.AdministradorId = administradorId;
            auditoria.HistoriaId = historia.Id;

            _context.Auditorias.Add(auditoria);
            _context.SaveChanges();
        }

        public void AgregarConAuditoria(
            Historia historia,
            CapituloIntermedio capituloInicial,
            int administradorId)
        {
            using var transaccion = _context.Database.BeginTransaction();

            try
            {
                // Las categorías ya existen en la base
                foreach (Categoria categoria in historia.Categorias)
                {
                    _context.Categorias.Attach(categoria);
                }

                // 1. Guardamos la historia
                _context.Historias.Add(historia);
                _context.SaveChanges();

                // 2. Agregamos el capítulo inicial
                historia.Capitulos.Add(capituloInicial);
                _context.SaveChanges();

                // 3. Indicamos cuál es el capítulo inicial
                historia.CapituloInicial = capituloInicial;
                _context.SaveChanges();

                // 4. Registramos la auditoría
                Auditoria auditoria = new Auditoria();

                auditoria.Accion = "Crear historia";
                auditoria.Fecha = DateTime.Now;
                auditoria.AdministradorId = administradorId;
                auditoria.HistoriaId = historia.Id;

                _context.Auditorias.Add(auditoria);
                _context.SaveChanges();

                // Validamos la historia completa antes de confirmar la creación
                capituloInicial.Validar();
                historia.Validar();
                auditoria.Validar();

                // 5. Confirmamos todos los cambios
                transaccion.Commit();
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }
        }

        public void AgregarCapituloInicial(
            Historia historia,
            CapituloIntermedio capitulo)
        {
            historia.Capitulos.Add(capitulo);
            _context.SaveChanges();

            historia.CapituloInicial = capitulo;
            _context.SaveChanges();
        }

        public void Eliminar(Historia historia)
        {
            // RF03 - No eliminamos historias que tengan lecturas asociadas
            if (TieneLecturas(historia.Id))
            {
                throw new ReglaNegocioException(
                    "No se puede eliminar una historia con lecturas");
            }

            using var transaccion = _context.Database.BeginTransaction();

            try
            {
                Historia? guardada = _context.Historias
                    .Include(h => h.Capitulos)
                    .FirstOrDefault(h => h.Id == historia.Id);

                if (guardada == null)
                {
                    throw new RecursoNoEncontradoException(
                        "La historia no existe");
                }

                List<int> ids = guardada.Capitulos
                    .Select(c => c.Id)
                    .ToList();

                // Eliminamos todas las opciones que llevan a capítulos
                // de la historia o salen de un capítulo intermedio de ella
                var opciones = _context.Opciones
                    .Where(o =>
                        (o.CapituloDestino != null &&
                         ids.Contains(o.CapituloDestino.Id)) ||
                        (Microsoft.EntityFrameworkCore.EF.Property<int?>(
                            o, "CapituloIntermedioId") != null &&
                         ids.Contains(
                            Microsoft.EntityFrameworkCore.EF.Property<int?>(
                                o, "CapituloIntermedioId").Value)))
                    .ToList();

                _context.Opciones.RemoveRange(opciones);

                guardada.CapituloInicial = null;
                _context.SaveChanges();

                _context.Capitulos.RemoveRange(guardada.Capitulos);
                _context.Historias.Remove(guardada);

                _context.SaveChanges();

                transaccion.Commit();
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }
        }

        public Historia? BuscarConEstructura(int id)
        {
            Historia? historia = _context.Historias
                .Include(h => h.CapituloInicial)
                .Include(h => h.Capitulos)
                .FirstOrDefault(h => h.Id == id);

            if (historia != null)
            {
                foreach (Capitulo capitulo in historia.Capitulos)
                {
                    if (capitulo is CapituloIntermedio intermedio)
                    {
                        _context.Entry(intermedio)
                            .Collection(i => i.Opciones)
                            .Load();

                        foreach (Opcion opcion in intermedio.Opciones)
                        {
                            _context.Entry(opcion)
                                .Reference(o => o.CapituloDestino)
                                .Load();
                        }
                    }
                }
            }

            return historia;
        }

        public bool TieneLecturas(int historiaId)
        {
            // Comprobamos directamente en SQL Server si existen lecturas
            return _context.Lecturas
                .Any(l => l.HistoriaId == historiaId);
        }
    }
}

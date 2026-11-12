
using Microsoft.EntityFrameworkCore;
using Obligatorio.LogicaAccesoDatos.EF;
using Obligatorio.LogicaNegocio.Entidades;

namespace Obligatorio.LogicaAccesoDatos.Repositorios
{
    public class RepositorioCapitulos
    {
        private ObligatorioContext _context = new ObligatorioContext();

        public Capitulo? BuscarPorId(int id)
        {
            return _context.Capitulos
                .FirstOrDefault(c => c.Id == id);
        }

        public void Agregar(Capitulo capitulo)
        {
            capitulo.Validar();
            _context.Capitulos.Add(capitulo);
            _context.SaveChanges();
        }

        public Capitulo? BuscarPorHistoria(int capituloId, int historiaId)
        {
            return _context.Historias
                .Where(h => h.Id == historiaId)
                .SelectMany(h => h.Capitulos)
                .FirstOrDefault(c => c.Id == capituloId);
        }

        public void AgregarCapituloAHistoria(
            int historiaId,
            int capituloAnteriorId,
            Capitulo nuevoCapitulo,
            string textoOpcion,
            int administradorId)
        {
            // VALIDACIONES DE CAMPOS OBLIGATORIOS

            if (nuevoCapitulo == null)
            {
                throw new ReglaNegocioException(
                    "El capítulo no puede ser nulo");
            }

            if (string.IsNullOrWhiteSpace(nuevoCapitulo.Titulo))
            {
                throw new ReglaNegocioException(
                    "El título del capítulo es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(nuevoCapitulo.Texto))
            {
                throw new ReglaNegocioException(
                    "El texto del capítulo es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(textoOpcion))
            {
                throw new ReglaNegocioException(
                    "El texto de la opción es obligatorio");
            }

            nuevoCapitulo.Validar();

            // BUSCAMOS LA HISTORIA

            Historia? historia = _context.Historias
                .Include(h => h.Capitulos)
                .FirstOrDefault(h => h.Id == historiaId);

            if (historia == null)
            {
                throw new RecursoNoEncontradoException(
                    "La historia no existe");
            }

            // EVITAMOS MODIFICAR HISTORIAS PUBLICADAS

            if (historia.Estado == EstadoHistoria.PUBLICADA)
            {
                throw new ReglaNegocioException(
                    "No se pueden agregar capítulos a una historia publicada");
            }

            // BUSCAMOS EL CAPÍTULO ANTERIOR

            Capitulo? anterior = null;

            foreach (Capitulo capitulo in historia.Capitulos)
            {
                if (capitulo.Id == capituloAnteriorId)
                {
                    anterior = capitulo;
                    break;
                }
            }

            if (anterior == null)
            {
                throw new ReglaNegocioException(
                    "El capítulo anterior no pertenece a la historia");
            }

            // VERIFICAMOS QUE SEA INTERMEDIO

            if (anterior is not CapituloIntermedio intermedio)
            {
                throw new ReglaNegocioException(
                    "No se pueden agregar capítulos después de un final");
            }

            // GUARDAMOS EL CAPÍTULO Y LA OPCIÓN EN UNA TRANSACCIÓN

            using var transaccion = _context.Database.BeginTransaction();

            try
            {
                // Agregamos el capítulo a la historia
                historia.Capitulos.Add(nuevoCapitulo);
                _context.SaveChanges();

                // Creamos la opción que conecta ambos capítulos
                Opcion opcion = new Opcion();

                opcion.Texto = textoOpcion;
                opcion.CapituloDestino = nuevoCapitulo;

                opcion.Validar();

                // Agregamos la opción
                _context.Opciones.Add(opcion);
                intermedio.AgregarOpcion(opcion);

                _context.SaveChanges();

                // Registramos la creación del capítulo en la misma transacción
                Auditoria auditoria = new Auditoria();

                auditoria.Accion = "Crear capítulo: " +
                    nuevoCapitulo.Titulo + " (Id " + nuevoCapitulo.Id + ")";
                auditoria.Fecha = DateTime.Now;
                auditoria.AdministradorId = administradorId;
                auditoria.HistoriaId = historia.Id;

                auditoria.Validar();

                _context.Auditorias.Add(auditoria);
                _context.SaveChanges();

                // Confirmamos los cambios
                transaccion.Commit();
            }
            catch
            {
                // Si ocurre un error, deshacemos los cambios
                transaccion.Rollback();
                throw;
            }
        }

        public List<CapituloIntermedio> ListarIntermedios(int historiaId)
        {
            List<CapituloIntermedio> resultado =
                new List<CapituloIntermedio>();

            // Buscamos la historia y sus capítulos
            Historia? historia = _context.Historias
                .Include(h => h.Capitulos)
                .FirstOrDefault(h => h.Id == historiaId);

            if (historia != null)
            {
                foreach (Capitulo capitulo in historia.Capitulos)
                {
                    if (capitulo is CapituloIntermedio intermedio)
                    {
                        resultado.Add(intermedio);
                    }
                }
            }

            return resultado;
        }

        // RF04/RF05 - Editamos un capítulo y registramos quién lo hizo
        public void EditarConAuditoria(
            int historiaId,
            int capituloId,
            string titulo,
            string texto,
            int administradorId)
        {
            if (string.IsNullOrWhiteSpace(titulo) ||
                string.IsNullOrWhiteSpace(texto))
            {
                throw new ReglaNegocioException(
                    "Título y texto son obligatorios");
            }

            Historia? historia = _context.Historias
                .Include(h => h.Capitulos)
                .FirstOrDefault(h => h.Id == historiaId);

            if (historia == null)
            {
                throw new RecursoNoEncontradoException(
                    "La historia no existe");
            }

            Capitulo? capitulo = null;

            foreach (Capitulo actual in historia.Capitulos)
            {
                if (actual.Id == capituloId)
                {
                    capitulo = actual;
                    break;
                }
            }

            if (capitulo == null)
            {
                throw new RecursoNoEncontradoException(
                    "El capítulo no pertenece a la historia");
            }

            using var transaccion = _context.Database.BeginTransaction();

            try
            {
                capitulo.Titulo = titulo;
                capitulo.Texto = texto;
                capitulo.Validar();

                _context.SaveChanges();

                Auditoria auditoria = new Auditoria();

                auditoria.Accion = "Editar capítulo: " +
                    titulo + " (Id " + capitulo.Id + ")";
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
    }
}

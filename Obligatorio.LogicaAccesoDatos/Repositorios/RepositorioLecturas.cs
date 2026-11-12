
using Obligatorio.LogicaAccesoDatos.EF;
using Obligatorio.LogicaNegocio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Obligatorio.LogicaAccesoDatos.Repositorios
{
    public class RepositorioLecturas
    {
        private ObligatorioContext _context =
            new ObligatorioContext();

        // Guardamos una nueva lectura en la base de datos
        public void Agregar(Lectura lectura)
        {
            lectura.Validar();
            _context.Lecturas.Add(lectura);
            _context.SaveChanges();
        }

        // Buscamos una lectura por su ID usando LINQ to Entities
        public Lectura? BuscarPorId(int id)
        {
            return _context.Lecturas
                .FirstOrDefault(l => l.Id == id);
        }

        public Capitulo ElegirOpcion(
            int lecturaId,
            int usuarioId,
            int opcionId)
        {
            using var transaccion = _context.Database.BeginTransaction(
                System.Data.IsolationLevel.Serializable);

            try
            {
                // Buscamos la lectura con su historia y capítulos
                Lectura? lectura = _context.Lecturas
                    .Include(l => l.Historia)
                    .ThenInclude(h => h.Capitulos)
                    .FirstOrDefault(l => l.Id == lecturaId);

                if (lectura == null)
                {
                    throw new RecursoNoEncontradoException(
                        "La lectura no existe");
                }

                // Verificamos que pertenezca al lector autenticado
                if (lectura.UsuarioId != usuarioId)
                {
                    throw new AccesoDenegadoException(
                        "La lectura no pertenece al usuario");
                }

                // Una lectura terminada no puede continuar
                if (lectura.FechaFinalizacion != null)
                {
                    throw new ReglaNegocioException(
                        "La lectura ya finalizó");
                }

                // Buscamos el capítulo actual
                Capitulo? actual = null;

                foreach (Capitulo capitulo in lectura.Historia.Capitulos)
                {
                    if (capitulo.Id == lectura.CapituloActualId)
                    {
                        actual = capitulo;
                        break;
                    }
                }

                if (actual is not CapituloIntermedio intermedio)
                {
                    throw new ReglaNegocioException(
                        "El capítulo actual no permite elegir opciones");
                }

                // Cargamos las opciones del capítulo actual
                _context.Entry(intermedio)
                    .Collection(i => i.Opciones)
                    .Load();

                // Buscamos la opción seleccionada
                Opcion? opcionElegida = null;

                foreach (Opcion opcion in intermedio.Opciones)
                {
                    if (opcion.Id == opcionId)
                    {
                        opcionElegida = opcion;
                        break;
                    }
                }

                if (opcionElegida == null)
                {
                    throw new ReglaNegocioException(
                        "La opción no pertenece al capítulo actual");
                }

                // Cargamos el capítulo al que lleva la opción
                _context.Entry(opcionElegida)
                    .Reference(o => o.CapituloDestino)
                    .Load();

                Capitulo? destino = opcionElegida.CapituloDestino;

                if (destino == null ||
                    !lectura.Historia.Capitulos.Any(c => c.Id == destino.Id))
                {
                    throw new ReglaNegocioException(
                        "El capítulo destino no es válido");
                }

                // Actualizamos el progreso
                lectura.CapituloActualId = destino.Id;

                // Si llegamos a un final, terminamos la lectura
                if (destino is CapituloFinal capituloFinal)
                {
                    lectura.FechaFinalizacion = DateTime.Now;
                    lectura.FinalAlcanzado = capituloFinal;

                    capituloFinal.IncrementarCuenta();
                }

                _context.SaveChanges();
                transaccion.Commit();

                return destino;
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }
        }

        public Capitulo ObtenerCapituloActual(int lecturaId, int usuarioId)
        {
            // Buscamos la lectura y cargamos los capítulos de su historia
            Lectura? lectura = _context.Lecturas
                .Include(l => l.Historia)
                .ThenInclude(h => h.Capitulos)
                .FirstOrDefault(l => l.Id == lecturaId);

            if (lectura == null)
            {
                throw new RecursoNoEncontradoException(
                    "La lectura no existe");
            }

            // Comprobamos que pertenezca al lector autenticado
            if (lectura.UsuarioId != usuarioId)
            {
                throw new AccesoDenegadoException(
                    "La lectura no pertenece al usuario");
            }

            // Buscamos el capítulo actual
            foreach (Capitulo capitulo in lectura.Historia.Capitulos)
            {
                if (capitulo.Id == lectura.CapituloActualId)
                {
                    // Si es intermedio, cargamos sus opciones
                    if (capitulo is CapituloIntermedio intermedio)
                    {
                        _context.Entry(intermedio)
                            .Collection(i => i.Opciones)
                            .Load();
                    }

                    return capitulo;
                }
            }

            throw new RecursoNoEncontradoException(
                "No se encontró el capítulo actual");
        }

        // RF08 - Listamos solamente las lecturas del lector autenticado
        public List<Lectura> ListarPorUsuario(int usuarioId, int mes, int anio)
        {
            DateTime desde = new DateTime(anio, mes, 1);
            DateTime hasta = desde.AddMonths(1);

            return _context.Lecturas
                .AsNoTracking()
                .Include(l => l.Historia)
                .Include(l => l.FinalAlcanzado)
                .Where(l => l.UsuarioId == usuarioId &&
                    l.FechaInicio >= desde &&
                    l.FechaInicio < hasta)
                .OrderByDescending(l => l.FechaInicio)
                .ToList();
        }
    }
}

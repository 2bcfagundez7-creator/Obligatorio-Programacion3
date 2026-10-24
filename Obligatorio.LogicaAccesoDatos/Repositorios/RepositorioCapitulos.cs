
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
            foreach (Capitulo capitulo in _context.Capitulos)
            {
                if (capitulo.Id == id)
                {
                    return capitulo;
                }
            }

            return null;
        }

        public void Agregar(Capitulo capitulo)
        {
            _context.Capitulos.Add(capitulo);
            _context.SaveChanges();
        }


        public Capitulo? BuscarPorHistoria(int capituloId, int historiaId)
        {
            foreach (Historia historia in _context.Historias
                .Include(h => h.Capitulos))
            {
                if (historia.Id == historiaId)
                {
                    foreach (Capitulo capitulo in historia.Capitulos)
                    {
                        if (capitulo.Id == capituloId)
                        {
                            return capitulo;
                        }
                    }
                }
            }

            return null;
        }


        public void AgregarCapituloAHistoria(int historiaId, int capituloAnteriorId,
            Capitulo nuevoCapitulo, string textoOpcion)
        {
            // Buscamos la historia
            Historia? historia = _context.Historias
                .Include(h => h.Capitulos)
                .FirstOrDefault(h => h.Id == historiaId);

            if (historia == null)
            {
                throw new Exception("La historia no existe");
            }

            // Buscamos el capítulo anterior dentro de esa historia
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
                throw new Exception("El capítulo anterior no pertenece a la historia");
            }

            // Solamente los capítulos intermedios pueden tener opciones
            if (anterior is not CapituloIntermedio intermedio)
            {
                throw new Exception("No se pueden agregar capítulos después de un final");
            }


            using var transaccion = _context.Database.BeginTransaction();

            try
            {
                historia.Capitulos.Add(nuevoCapitulo);
                _context.SaveChanges();

                Opcion opcion = new Opcion();
                opcion.Texto = textoOpcion;
                opcion.CapituloDestino = nuevoCapitulo;

                _context.Opciones.Add(opcion);
                intermedio.AgregarOpcion(opcion);

                _context.SaveChanges();

                transaccion.Commit();
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }

        }

        public List<CapituloIntermedio> ListarIntermedios(int historiaId)
        {
            // Buscamos la historia y sus capítulos intermedios
            List<CapituloIntermedio> resultado = new List<CapituloIntermedio>();
            // Buscamos la historia
            Historia? historia = _context.Historias
                // Incluimos los capítulos para poder acceder a ellos
                .Include(h => h.Capitulos)
                // Filtramos por el id de la historia
                .FirstOrDefault(h => h.Id == historiaId);

            if (historia != null)
            {
                // Recorremos los capítulos de la historia y agregamos los intermedios a la lista
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



    }
}

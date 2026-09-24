using Microsoft.EntityFrameworkCore;
using Obligatorio.LogicaAccesoDatos.EF;
using Obligatorio.LogicaNegocio.Entidades;

namespace Obligatorio.LogicaAccesoDatos.Repositorios
{
    public class RepositorioHistorias
    {
        private ObligatorioContext _context = new ObligatorioContext();

        public List<Historia> Listar()
        {
            return _context.Historias.ToList();
        }
        public Historia? BuscarPorId(int id)
        {
            // Cargamos las historias con sus categorías, capítulos y capítulo inicial
            foreach (Historia historia in _context.Historias
                .Include(h => h.Categorias)
                .Include(h => h.Capitulos)
                .Include(h => h.CapituloInicial))
            {
                if (historia.Id == id)
                {
                    return historia;
                }
            }

            return null;
        }
        public void ActualizarCategorias(Historia historia, List<int> categoriasIds)
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
                foreach (Categoria categoria in _context.Categorias)
                {
                    if (categoria.Id == categoriaId)
                    {
                        historia.Categorias.Add(categoria);
                        break;
                    }
                }
            }

            _context.SaveChanges();
        }
        public void Modificar()
        {
            _context.SaveChanges();
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

        public void AgregarCapituloInicial(Historia historia, CapituloIntermedio capitulo)
        {
            historia.Capitulos.Add(capitulo);

            _context.SaveChanges();

            historia.CapituloInicial = capitulo;

            _context.SaveChanges();
        }
        public void Eliminar(Historia historia)
        {
            // Primero rompemos la relación con el capítulo inicial
            historia.CapituloInicial = null;
            _context.SaveChanges();

            _context.Historias.Remove(historia);
            _context.SaveChanges();
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
            foreach (Lectura lectura in _context.Lecturas)
            {
                if (lectura.HistoriaId == historiaId)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
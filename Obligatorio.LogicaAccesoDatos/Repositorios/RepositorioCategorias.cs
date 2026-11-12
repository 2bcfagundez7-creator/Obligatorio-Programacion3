
using Obligatorio.LogicaAccesoDatos.EF;
using Obligatorio.LogicaNegocio.Entidades;

namespace Obligatorio.LogicaAccesoDatos.Repositorios
{
    public class RepositorioCategorias
    {
        private ObligatorioContext _context = new ObligatorioContext();

        public List<Categoria> Listar()
        {
            return _context.Categorias.ToList();
        }

        public Categoria? BuscarPorId(int id)
        {
            // Buscamos la categoría por su ID utilizando LINQ to Entities
            return _context.Categorias
                .FirstOrDefault(c => c.Id == id);
        }
    }
}

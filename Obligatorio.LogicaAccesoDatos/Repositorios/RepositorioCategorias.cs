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
            foreach (Categoria categoria in _context.Categorias)
            {
                if (categoria.Id == id)
                {
                    return categoria;
                }
            }

            return null;
        }
    }
}
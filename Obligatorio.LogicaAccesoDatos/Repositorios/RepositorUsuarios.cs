using Microsoft.EntityFrameworkCore;
using Obligatorio.LogicaAccesoDatos.EF;
using Obligatorio.LogicaNegocio.Entidades;

namespace Obligatorio.LogicaAccesoDatos.Repositorios
{
    public class RepositorioUsuarios
    {
        private ObligatorioContext _context = new ObligatorioContext();

        public Administrador? LoginAdministrador(string username, string password)
        {
            return _context.Administradores
                .FirstOrDefault(a => a.Username == username && a.Password == password);
        }

        public bool ExisteUsername(string username)
        {
            foreach (Usuario usuario in _context.Usuarios)
            {
                if (usuario.Username == username)
                {
                    return true;
                }
            }

            return false;
        }

        public bool ExisteEmail(string email)
        {
            bool existe = false;

            foreach (Usuario usuario in _context.Usuarios)
            {
                if (usuario.Email == email)
                {
                    existe = true;
                }
            }

            return existe;
        }
        public void Agregar(Usuario usuario)
        {
            // Agregar el usuario a la base de datos
            _context.Usuarios.Add(usuario);
            // Guardar los cambios en la base de datoss
            _context.SaveChanges();
        }
    }

}


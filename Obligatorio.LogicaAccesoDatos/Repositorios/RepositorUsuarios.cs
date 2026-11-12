using Microsoft.EntityFrameworkCore;
using Obligatorio.LogicaAccesoDatos.EF;
using Obligatorio.LogicaNegocio.Entidades;

namespace Obligatorio.LogicaAccesoDatos.Repositorios
{
    public class RepositorioUsuarios
    {
        private ObligatorioContext _context = new ObligatorioContext();

        // RF01 / RF06: el mismo usuario puede ingresar a la aplicación correspondiente a su rol.
        public Usuario? LoginUsuario(string username, string password)
        {
            Usuario? usuario = _context.Usuarios.FirstOrDefault(u => u.Username == username);
            if (usuario == null || !usuario.VerificarPassword(password)) return null;

            // Si era un usuario de la precarga antigua, guardamos el hash al ingresar.
            if (!usuario.Password.StartsWith("PBKDF2$", StringComparison.Ordinal))
            {
                usuario.ProtegerPassword();
                _context.SaveChanges();
            }
            return usuario;
        }

        public Administrador? LoginAdministrador(string username, string password)
        {
            return LoginUsuario(username, password) as Administrador;
        }

        public Lector? LoginLector(string username, string password)
        {
            return LoginUsuario(username, password) as Lector;
        }

        public bool ExisteUsername(string username)
        {
            return _context.Usuarios.Any(u => u.Username.ToLower() == username.ToLower());
        }

        public bool ExisteEmail(string email)
        {
            return _context.Usuarios.Any(u => u.Email.ToLower() == email.ToLower());
        }

        public void Agregar(Usuario usuario)
        {
            usuario.Validar();
            if (ExisteUsername(usuario.Username))
                throw new ReglaNegocioException("El nombre de usuario ya existe");
            if (ExisteEmail(usuario.Email))
                throw new ReglaNegocioException("El correo electrónico ya existe");

            usuario.ProtegerPassword();
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
        }
    }
}

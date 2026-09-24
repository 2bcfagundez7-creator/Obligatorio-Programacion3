using System.Text.RegularExpressions;

namespace Obligatorio.LogicaNegocio.Entidades
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;

        public Usuario() { }

        public void SetPassword(string password)
        {
            Password = password ?? string.Empty;
        }

        public bool ValidarPassword()
        {
            var pwd = Password ?? string.Empty;
            if (pwd.Length < 8) return false;
            if (!Regex.IsMatch(pwd, "[A-Z]")) return false;
            if (!Regex.IsMatch(pwd, "[a-z]")) return false;
            if (!Regex.IsMatch(pwd, "[0-9]")) return false;
            if (!Regex.IsMatch(pwd, "[^a-zA-Z0-9]")) return false;
            return true;
        }
    }
}

using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace Obligatorio.LogicaNegocio.Entidades
{
    public class Usuario : IValidable
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

        public void Validar()
        {
            if (string.IsNullOrWhiteSpace(NombreCompleto) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Username))
            {
                throw new ReglaNegocioException("Los datos del usuario son obligatorios");
            }
            if (!Email.Contains('@') || Email.StartsWith('@') || Email.EndsWith('@'))
            {
                throw new ReglaNegocioException("El correo electrónico no es válido");
            }
            if (!ValidarPassword())
            {
                throw new ReglaNegocioException("La contraseña debe tener 8 caracteres, mayúscula, minúscula, número y símbolo");
            }
        }

        // Protegemos la contraseña con PBKDF2 y una sal diferente por usuario.
        // Se hace después de validar la contraseña de texto ingresada.
        public void ProtegerPassword()
        {
            if (Password.StartsWith("PBKDF2$", StringComparison.Ordinal)) return;

            byte[] sal = RandomNumberGenerator.GetBytes(16);
            int iteraciones = 150000;
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Password, sal, iteraciones, HashAlgorithmName.SHA256, 32);
            Password = "PBKDF2$" + iteraciones + "$" +
                Convert.ToBase64String(sal) + "$" + Convert.ToBase64String(hash);
        }

        public bool VerificarPassword(string passwordIngresada)
        {
            if (!Password.StartsWith("PBKDF2$", StringComparison.Ordinal))
            {
                // Compatibilidad con usuarios creados antes de incorporar el hash.
                return Password == passwordIngresada;
            }

            try
            {
                string[] partes = Password.Split('$');
                if (partes.Length != 4 || !int.TryParse(partes[1], out int iteraciones))
                    return false;
                if (iteraciones < 100000 || iteraciones > 1000000) return false;
                byte[] sal = Convert.FromBase64String(partes[2]);
                byte[] guardado = Convert.FromBase64String(partes[3]);
                byte[] calculado = Rfc2898DeriveBytes.Pbkdf2(
                    passwordIngresada, sal, iteraciones, HashAlgorithmName.SHA256, guardado.Length);
                return CryptographicOperations.FixedTimeEquals(guardado, calculado);
            }
            catch (FormatException)
            {
                return false;
            }
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Obligatorio.Api.DTOs;
using Obligatorio.LogicaAccesoDatos.Repositorios;
using Obligatorio.LogicaNegocio.Entidades;

namespace Obligatorio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private RepositorioUsuarios _repoUsuarios =
            new RepositorioUsuarios();

        // Nos permite leer la configuración de appsettings.json
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO dto)
        {
            // Validamos que se hayan enviado los datos
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Usuario y contraseña obligatorios");
            }

            // RF06 - Tanto los administradores como los lectores pueden ingresar.
            Usuario? usuario = _repoUsuarios.LoginUsuario(
                dto.Username,
                dto.Password
            );

            if (usuario == null)
            {
                return Unauthorized("Credenciales incorrectas");
            }

            // Obtenemos los datos JWT de appsettings.json
            var jwt = _configuration.GetSection("Jwt");

            string clave = jwt["Key"]
                ?? throw new Exception("Falta la clave JWT");

            // Creamos los datos que identifican al lector
            // ¿Qué es un Claim? Es un dato que identifica al usuario y que se guarda en el token.
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(
                ClaimTypes.NameIdentifier,
                usuario.Id.ToString()
            ));

            claims.Add(new Claim(
                ClaimTypes.Name,
                usuario.Username
            ));

            claims.Add(new Claim(
                ClaimTypes.Role,
                usuario is Administrador ? "Administrador" : "Lector"
            ));

            // Preparamos la clave para firmar el token
            SymmetricSecurityKey securityKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(clave)
                );

            SigningCredentials credenciales =
                new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.HmacSha256
                );

            // Obtenemos la duración del token
            int minutos = int.Parse(
                jwt["ExpirationMinutes"] ?? "60"
            );

            // Creamos el token JWT
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutos),
                signingCredentials: credenciales
            );

            // Convertimos el token a texto
            string tokenGenerado =
                new JwtSecurityTokenHandler()
                    .WriteToken(token);

            // Devolvemos el token al cliente
            return Ok(new
            {
                token = tokenGenerado,
                usuarioId = usuario.Id,
                username = usuario.Username,
                rol = usuario is Administrador ? "Administrador" : "Lector"
            });
        }
    }
}

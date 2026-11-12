
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Obligatorio.LogicaAccesoDatos.Repositorios;
using Obligatorio.LogicaNegocio.Entidades;
using System.Security.Claims;

namespace Obligatorio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Lector")]
    public class LecturasController : ControllerBase
    {
        // Acción para obtener los datos del lector autenticado
        [HttpGet("mis-datos")]
        public IActionResult MisDatos()
        {
            // Obtenemos el ID del lector desde su token JWT
            string? usuarioId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioId, out int lectorId))
            {
                return Unauthorized();
            }

            // Obtenemos el nombre del lector desde su token
            // Acá la API sabe quién es el lector porque el token fue generado cuando se autenticó.
            string? username = User.FindFirst(
                ClaimTypes.Name)?.Value;

            return Ok(new
            {
                mensaje = "El lector está autenticado",
                usuarioId = lectorId,
                username = username
            });
        }

        [HttpPost("iniciar/{historiaId}")]
        public IActionResult IniciarLectura(int historiaId)
        {
            // Obtenemos el ID del lector desde el token JWT
            // El User.FindFirst(ClaimTypes.NameIdentifier) nos permite acceder al claim que contiene el ID del usuario.
            string? usuarioId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioId, out int lectorId))
            {
                return Unauthorized();
            }

            if (historiaId <= 0)
            {
                return BadRequest("El ID de la historia no es válido");
            }

            try
            {
                // Buscamos la historia en la base de datos
                RepositorioHistorias repoHistorias =
                    new RepositorioHistorias();

                Historia? historia = repoHistorias.BuscarPorId(historiaId);

                if (historia == null)
                {
                    return NotFound("La historia no existe");
                }

                // Solo se pueden leer historias publicadas
                if (historia.Estado != EstadoHistoria.PUBLICADA)
                {
                    return BadRequest("La historia no está publicada");
                }

                // Comprobamos que exista un capítulo inicial
                if (historia.CapituloInicial == null)
                {
                    return BadRequest("La historia no tiene capítulo inicial");
                }

                // Creamos una nueva lectura
                Lectura lectura = new Lectura();

                lectura.UsuarioId = lectorId;
                lectura.HistoriaId = historia.Id;
                lectura.FechaInicio = DateTime.Now;

                // El lector comienza en el capítulo inicial
                lectura.CapituloActualId = historia.CapituloInicial.Id;

                // Guardamos la lectura en SQL Server
                RepositorioLecturas repoLecturas =
                    new RepositorioLecturas();

                lectura.Validar();
                repoLecturas.Agregar(lectura);

                return Ok(new
                {
                    mensaje = "Lectura iniciada",
                    historiaId = historia.Id,
                    tituloHistoria = historia.Titulo,
                    capituloId = historia.CapituloInicial.Id,
                    tituloCapitulo = historia.CapituloInicial.Titulo,
                    textoCapitulo = historia.CapituloInicial.Texto,
                    lecturaId = lectura.Id
                });
            }
            catch (RecursoNoEncontradoException e)
            {
                return NotFound(e.Message);
            }
            catch (AccesoDenegadoException)
            {
                return Forbid();
            }
            catch (ReglaNegocioException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("{lecturaId}/elegir/{opcionId}")]
        public IActionResult ElegirOpcion(int lecturaId, int opcionId)
        {
            // Obtenemos el ID del lector autenticado desde el JWT
            string? usuarioId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioId, out int lectorId))
            {
                return Unauthorized();
            }

            if (lecturaId <= 0 || opcionId <= 0)
            {
                return BadRequest("Los identificadores no son válidos");
            }

            // Creamos el repositorio de lecturas
            RepositorioLecturas repoLecturas =
                new RepositorioLecturas();

            try
            {
                // Ejecutamos la elección de la opción
                Capitulo capitulo = repoLecturas.ElegirOpcion(
                    lecturaId,
                    lectorId,
                    opcionId
                );

                // Devolvemos los datos del capítulo al que avanzamos
                return Ok(new
                {
                    mensaje = "Opción elegida correctamente",
                    capituloId = capitulo.Id,
                    titulo = capitulo.Titulo,
                    texto = capitulo.Texto,
                    esFinal = capitulo.EsFinal
                });
            }
            catch (RecursoNoEncontradoException e)
            {
                return NotFound(e.Message);
            }
            catch (AccesoDenegadoException)
            {
                return Forbid();
            }
            catch (ReglaNegocioException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{lecturaId}/actual")]
        public IActionResult ObtenerCapituloActual(int lecturaId)
        {
            // Obtenemos el ID del lector desde el JWT
            string? usuarioId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioId, out int lectorId))
            {
                return Unauthorized();
            }

            if (lecturaId <= 0)
            {
                return BadRequest("El ID de la lectura no es válido");
            }

            RepositorioLecturas repoLecturas =
                new RepositorioLecturas();

            try
            {
                Capitulo capitulo = repoLecturas.ObtenerCapituloActual(
                    lecturaId,
                    lectorId
                );

                // Preparamos las opciones para devolverlas como JSON
                List<object> opciones = new List<object>();

                if (capitulo is CapituloIntermedio intermedio)
                {
                    foreach (Opcion opcion in intermedio.Opciones)
                    {
                        opciones.Add(new
                        {
                            id = opcion.Id,
                            texto = opcion.Texto
                        });
                    }
                }

                return Ok(new
                {
                    capituloId = capitulo.Id,
                    titulo = capitulo.Titulo,
                    texto = capitulo.Texto,
                    esFinal = capitulo.EsFinal,
                    opciones = opciones
                });
            }
            catch (RecursoNoEncontradoException e)
            {
                return NotFound(e.Message);
            }
            catch (AccesoDenegadoException)
            {
                return Forbid();
            }
            catch (ReglaNegocioException e)
            {
                return BadRequest(e.Message);
            }
        }

        // RF08 - Historial de lecturas del usuario del JWT
        [HttpGet("historial")]
        public IActionResult Historial([FromQuery] int mes, [FromQuery] int anio)
        {
            string? usuarioId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioId, out int lectorId))
            {
                return Unauthorized();
            }

            if (mes < 1 || mes > 12 || anio < 1900 || anio > 2100)
            {
                return BadRequest("Debés ingresar un mes y año válidos");
            }

            try
            {
                RepositorioLecturas repoLecturas =
                    new RepositorioLecturas();

                List<Lectura> lecturas =
                    repoLecturas.ListarPorUsuario(lectorId, mes, anio);

                List<object> resultado = new List<object>();

                foreach (Lectura lectura in lecturas)
                {
                    resultado.Add(new
                    {
                        lecturaId = lectura.Id,
                        historiaId = lectura.HistoriaId,
                        tituloHistoria = lectura.Historia.Titulo,
                        fechaInicio = lectura.FechaInicio,
                        fechaFinalizacion = lectura.FechaFinalizacion,
                        nombreFinal = lectura.FinalAlcanzado?.Titulo,
                        finalizada = lectura.FechaFinalizacion != null
                    });
                }

                return Ok(resultado);
            }
            catch (RecursoNoEncontradoException e)
            {
                return NotFound(e.Message);
            }
            catch (AccesoDenegadoException)
            {
                return Forbid();
            }
            catch (ReglaNegocioException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}

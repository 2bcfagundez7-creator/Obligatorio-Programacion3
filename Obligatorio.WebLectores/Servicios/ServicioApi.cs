using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Obligatorio.WebLectores.Servicios
{
    // Recibe el resultado de una solicitud a nuestra API
    public class ResultadoApi<T>
    {
        public bool Correcto { get; set; }
        public T? Datos { get; set; }
        public string Error { get; set; } = "";
        public HttpStatusCode Codigo { get; set; }
    }

    public class ServicioApi
    {
        private readonly HttpClient _cliente;

        public ServicioApi(HttpClient cliente)
        {
            _cliente = cliente;
        }

        public async Task<ResultadoApi<T>> GetAsync<T>(string ruta, string? token = null)
        {
            return await EnviarAsync<T>(HttpMethod.Get, ruta, token, null);
        }

        public async Task<ResultadoApi<T>> PostAsync<T>(string ruta, string? token = null, object? datos = null)
        {
            return await EnviarAsync<T>(HttpMethod.Post, ruta, token, datos);
        }

        private async Task<ResultadoApi<T>> EnviarAsync<T>(
            HttpMethod metodo, string ruta, string? token, object? datos)
        {
            ResultadoApi<T> resultado = new ResultadoApi<T>();

            try
            {
                using HttpRequestMessage solicitud = new HttpRequestMessage(metodo, ruta);

                // Si el lector inició sesión, enviamos su JWT a la API
                if (!string.IsNullOrEmpty(token))
                {
                    solicitud.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                if (datos != null)
                {
                    solicitud.Content = JsonContent.Create(datos);
                }

                using HttpResponseMessage respuesta = await _cliente.SendAsync(solicitud);
                resultado.Codigo = respuesta.StatusCode;

                if (!respuesta.IsSuccessStatusCode)
                {
                    if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        resultado.Error = "La sesión venció o las credenciales son incorrectas";
                    }
                    else if (respuesta.StatusCode == HttpStatusCode.Forbidden)
                    {
                        resultado.Error = "No tenés permiso para acceder";
                    }
                    else
                    {
                        string detalle = await respuesta.Content.ReadAsStringAsync();
                        resultado.Error = !string.IsNullOrWhiteSpace(detalle) && detalle.Length < 300
                            ? detalle.Trim('"')
                            : "La API devolvió un error: " + (int)respuesta.StatusCode;
                    }
                    return resultado;
                }

                // Convertimos el JSON de respuesta en el DTO que necesitamos
                resultado.Datos = await respuesta.Content.ReadFromJsonAsync<T>();
                resultado.Correcto = true;
            }
            catch (HttpRequestException)
            {
                resultado.Error = "No se pudo conectar con la API. Comprobá que esté ejecutándose";
            }
            catch (TaskCanceledException)
            {
                resultado.Error = "La API demoró demasiado en responder";
            }
            catch (JsonException)
            {
                resultado.Error = "La API devolvió datos que no se pudieron leer";
            }

            return resultado;
        }
    }
}

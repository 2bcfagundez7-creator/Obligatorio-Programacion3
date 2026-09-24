using System.Collections.Generic;
using System.Linq;

namespace Obligatorio.LogicaNegocio.Entidades
{
    public class Historia
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Sinopsis { get; set; } = string.Empty;
        public List<Categoria> Categorias { get; } = new List<Categoria>();
        public EstadoHistoria Estado { get; set; } = EstadoHistoria.BORRADOR;
        public Capitulo? CapituloInicial { get; set; }
        public List<Capitulo> Capitulos { get; } = new List<Capitulo>();

        public bool ValidarEstructura()
        {
            // Tiene que existir un capítulo inicial
            if (CapituloInicial == null)
            {
                return false;
            }

            // El capítulo inicial no puede ser final
            if (CapituloInicial is CapituloFinal)
            {
                return false;
            }

            // El capítulo inicial tiene que pertenecer a la historia
            if (!Capitulos.Contains(CapituloInicial))
            {
                return false;
            }

            // Revisamos todos los capítulos intermedios
            foreach (Capitulo capitulo in Capitulos)
            {
                if (capitulo is CapituloIntermedio intermedio)
                {
                    // Todo capítulo intermedio debe tener al menos una opción
                    if (intermedio.Opciones.Count == 0)
                    {
                        return false;
                    }

                    // Todas las opciones deben tener un capítulo destino
                    foreach (Opcion opcion in intermedio.Opciones)
                    {
                        if (opcion.CapituloDestino == null)
                        {
                            return false;
                        }

                        // El destino tiene que pertenecer a esta misma historia
                        if (!Capitulos.Contains(opcion.CapituloDestino))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}

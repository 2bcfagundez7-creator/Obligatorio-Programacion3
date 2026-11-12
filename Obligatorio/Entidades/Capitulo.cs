using System;

namespace Obligatorio.LogicaNegocio.Entidades
{
    public abstract class Capitulo : IValidable
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;

        public abstract bool EsFinal { get; }

        public virtual void Validar()
        {
            if (string.IsNullOrWhiteSpace(Titulo) || string.IsNullOrWhiteSpace(Texto))
            {
                throw new ReglaNegocioException("El título y el texto del capítulo son obligatorios");
            }
        }
    }
}

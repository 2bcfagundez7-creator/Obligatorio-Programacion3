using System;

namespace Obligatorio.LogicaNegocio.Entidades
{
    public abstract class Capitulo
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;

        public abstract bool EsFinal { get; }
    }
}

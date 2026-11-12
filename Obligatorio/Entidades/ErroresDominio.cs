namespace Obligatorio.LogicaNegocio.Entidades
{
    public class ReglaNegocioException : Exception
    {
        public ReglaNegocioException(string mensaje) : base(mensaje) { }
    }

    public class RecursoNoEncontradoException : Exception
    {
        public RecursoNoEncontradoException(string mensaje) : base(mensaje) { }
    }

    public class AccesoDenegadoException : Exception
    {
        public AccesoDenegadoException(string mensaje) : base(mensaje) { }
    }
}

namespace Obligatorio.LogicaNegocio.Entidades
{
    public class Administrador : Usuario
    {
        public Administrador() { }

        public bool ValidarEstructura(Historia historia)
        {
            return historia?.ValidarEstructura() ?? false;
        }

        public bool PublicarHistoria(Historia historia)
        {
            if (historia == null) return false;
            if (!ValidarEstructura(historia)) return false;
            historia.Estado = EstadoHistoria.PUBLICADA;
            return true;
        }
    }
}

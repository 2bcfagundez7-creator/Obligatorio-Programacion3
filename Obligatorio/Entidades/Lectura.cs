namespace Obligatorio.LogicaNegocio.Entidades
{
    public class Lectura
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public int HistoriaId { get; set; }
        public Historia Historia { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }

        public CapituloFinal? FinalAlcanzado { get; set; }
    }
}

namespace Obligatorio.LogicaNegocio.Entidades
{
    // Lo que hago acá es definir la entidad Lectura, que representa una sesión de lectura de un lector sobre una historia.
    public class Lectura : IValidable
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public int HistoriaId { get; set; }
        public Historia Historia { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public CapituloFinal? FinalAlcanzado { get; set; }
        public int? CapituloActualId { get; set; }

        public void Validar()
        {
            if (UsuarioId <= 0 || HistoriaId <= 0 || CapituloActualId == null)
                throw new ReglaNegocioException("La lectura necesita usuario, historia y capítulo actual");
            if (FechaFinalizacion != null && FinalAlcanzado == null)
                throw new ReglaNegocioException("Una lectura finalizada necesita un final alcanzado");
        }
    }
}

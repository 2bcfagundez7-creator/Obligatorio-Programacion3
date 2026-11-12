namespace Obligatorio.LogicaAccesoDatos.DTOs
{
    // DTOs con los datos que devolvemos para los rankings
    public class RankingHistoriaDatos
    {
        public int HistoriaId { get; set; }
        public string Titulo { get; set; } = "";
        public int LecturasIniciadas { get; set; }
        public int LecturasCompletadas { get; set; }
    }

    public class RankingFinalDatos
    {
        public int CapituloId { get; set; }
        public string TituloFinal { get; set; } = "";
        public int HistoriaId { get; set; }
        public string TituloHistoria { get; set; } = "";
        public int VecesAlcanzado { get; set; }
    }

    public class RankingLectorDatos
    {
        public int UsuarioId { get; set; }
        public string Username { get; set; } = "";
        public int HistoriasCompletadas { get; set; }
    }
}

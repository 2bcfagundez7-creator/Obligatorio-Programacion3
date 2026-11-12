namespace Obligatorio.WebLectores.Models
{
    // DTOs para recibir y enviar datos entre el MVC y la API
    public class LoginLectorDTO
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class TokenRespuestaDTO
    {
        public string Token { get; set; } = "";
        public int UsuarioId { get; set; }
        public string Username { get; set; } = "";
        public string Rol { get; set; } = "";
    }

    public class CategoriaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
    }

    public class HistoriaPublicadaDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Sinopsis { get; set; } = "";
        public List<string> Categorias { get; set; } = new List<string>();
    }

    public class LecturaIniciadaDTO
    {
        public int LecturaId { get; set; }
        public int HistoriaId { get; set; }
        public string TituloHistoria { get; set; } = "";
    }

    public class OpcionDTO
    {
        public int Id { get; set; }
        public string Texto { get; set; } = "";
    }

    public class CapituloActualDTO
    {
        public int CapituloId { get; set; }
        public string Titulo { get; set; } = "";
        public string Texto { get; set; } = "";
        public bool EsFinal { get; set; }
        public List<OpcionDTO> Opciones { get; set; } = new List<OpcionDTO>();
    }

    public class LecturaHistorialDTO
    {
        public int LecturaId { get; set; }
        public int HistoriaId { get; set; }
        public string TituloHistoria { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public string? NombreFinal { get; set; }
        public bool Finalizada { get; set; }
    }

    public class RankingHistoriaDTO
    {
        public int HistoriaId { get; set; }
        public string Titulo { get; set; } = "";
        public int LecturasIniciadas { get; set; }
        public int LecturasCompletadas { get; set; }
    }

    public class RankingFinalDTO
    {
        public int CapituloId { get; set; }
        public string TituloFinal { get; set; } = "";
        public int HistoriaId { get; set; }
        public string TituloHistoria { get; set; } = "";
        public int VecesAlcanzado { get; set; }
    }

    public class RankingLectorDTO
    {
        public int UsuarioId { get; set; }
        public string Username { get; set; } = "";
        public int HistoriasCompletadas { get; set; }
    }

    public class FinalesPorHistoriaDTO
    {
        public int HistoriaId { get; set; }
        public List<RankingFinalDTO> Finales { get; set; } = new List<RankingFinalDTO>();
        public List<int> MasFrecuentes { get; set; } = new List<int>();
    }

    public class RankingsDTO
    {
        public int? HistoriaSeleccionadaId { get; set; }
        public List<RankingHistoriaDTO> OpcionesHistorias { get; set; } = new List<RankingHistoriaDTO>();
        public List<int> FinalesMasFrecuentes { get; set; } = new List<int>();
        public List<RankingHistoriaDTO> Historias { get; set; } = new List<RankingHistoriaDTO>();
        public List<RankingFinalDTO> Finales { get; set; } = new List<RankingFinalDTO>();
        public List<RankingLectorDTO> Lectores { get; set; } = new List<RankingLectorDTO>();
        public string? Error { get; set; }
    }

    public class PreferenciaDTO
    {
        public string Estilo { get; set; } = "claro";
    }
}

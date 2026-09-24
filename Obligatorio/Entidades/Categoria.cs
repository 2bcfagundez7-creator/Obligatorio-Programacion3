using Obligatorio.LogicaNegocio.Entidades;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public List<Historia> Historias { get; } = new List<Historia>();
}

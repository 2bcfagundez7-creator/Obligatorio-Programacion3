using Obligatorio.LogicaNegocio.Entidades;

public class Categoria : IValidable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public List<Historia> Historias { get; } = new List<Historia>();

    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
            throw new Obligatorio.LogicaNegocio.Entidades.ReglaNegocioException("El nombre de la categoría es obligatorio");
    }
}

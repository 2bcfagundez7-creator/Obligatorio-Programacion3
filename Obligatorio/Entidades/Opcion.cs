namespace Obligatorio.LogicaNegocio.Entidades
{
    public class Opcion
    {
        public int Id { get; set; }
        public string Texto { get; set; } = string.Empty;
        public Capitulo? CapituloDestino { get; set; }
    }
}

namespace Obligatorio.LogicaNegocio.Entidades
{
    public class Opcion : IValidable
    {
        public int Id { get; set; }
        public string Texto { get; set; } = string.Empty;
        public Capitulo? CapituloDestino { get; set; }

        public void Validar()
        {
            if (string.IsNullOrWhiteSpace(Texto) || CapituloDestino == null)
            {
                throw new ReglaNegocioException("La opción necesita texto y un capítulo destino");
            }
        }
    }
}

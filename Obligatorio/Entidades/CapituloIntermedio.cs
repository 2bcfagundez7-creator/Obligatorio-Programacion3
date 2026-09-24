using System.Collections.Generic;
using System.Linq;

namespace Obligatorio.LogicaNegocio.Entidades
{
    public class CapituloIntermedio : Capitulo
    {
        public List<Opcion> Opciones { get; } = new List<Opcion>();

        public override bool EsFinal => false;

        public void AgregarOpcion(Opcion opcion)
        {
            if (opcion != null) Opciones.Add(opcion);
        }

        public bool TieneOpciones => Opciones.Any();
    }
}

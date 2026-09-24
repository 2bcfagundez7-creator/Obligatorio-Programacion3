namespace Obligatorio.LogicaNegocio.Entidades
{
    public class CapituloFinal : Capitulo
    {
        public int VecesAlcanzado { get; private set; }

        public override bool EsFinal => true;

        public void IncrementarCuenta()
        {
            VecesAlcanzado++;
        }
    }
}

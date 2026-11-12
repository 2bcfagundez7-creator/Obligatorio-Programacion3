using System;
using System.Collections.Generic;
using System.Text;


namespace Obligatorio.LogicaNegocio.Entidades
{
    public class Auditoria : IValidable
    {
        public int Id { get; set; }

        public string Accion { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public int AdministradorId { get; set; }
        public Administrador Administrador { get; set; } = null!;

        // Relación con Historia (opcional)
        public int? HistoriaId { get; set; }
        public Historia? Historia { get; set; }

        public void Validar()
        {
            if (string.IsNullOrWhiteSpace(Accion) || AdministradorId <= 0 || Fecha == default)
                throw new ReglaNegocioException("La auditoría necesita acción, fecha y administrador");
        }
    }
}


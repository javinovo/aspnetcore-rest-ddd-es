using Infrastructure.Domain;
using System;

namespace BoundedContext.Montajes.Events
{
    public class EquipoCreado : Event
    {
        public readonly string Nombre;

        public EquipoCreado(Guid id, string nombre) : base(id)
        {
            Nombre = nombre;
        }
    }

    public class NombreEquipoActualizado : Event
    {
        public readonly string NuevoNombre;

        public NombreEquipoActualizado(Guid id, string nuevoNombre) : base(id)
        {
            NuevoNombre = nuevoNombre;
        }
    }
}

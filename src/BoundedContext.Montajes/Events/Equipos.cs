using Infrastructure.Domain;
using System;

namespace BoundedContext.Montajes.Events
{
    public class EquipoCreado : Event
    {
        public readonly Guid Id;
        public readonly string Nombre;

        public EquipoCreado(Guid id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }
    }

    public class NombreEquipoActualizado : Event
    {
        public readonly Guid Id;
        public readonly string NuevoNombre;

        public NombreEquipoActualizado(Guid id, string nuevoNombre)
        {
            Id = id;
            NuevoNombre = nuevoNombre;
        }
    }
}

using Infrastructure.Domain.Interfaces;
using System;

namespace BoundedContext.Montajes.Commands
{
    public class CrearEquipo : ICommand
    {
        public readonly Guid EquipoId;
        public readonly string Nombre;

        public CrearEquipo(Guid id, string nombre)
        {
            EquipoId = id;
            Nombre = nombre;
        }
    }

    public class ActualizarNombreEquipo : ICommand
    {
        public readonly Guid EquipoId;
        public readonly string NuevoNombre;
        public readonly int OriginalVersion;

        public ActualizarNombreEquipo(Guid id, string name, int originalVersion)
        {
            EquipoId = id;
            NuevoNombre = name;
            OriginalVersion = originalVersion;
        }
    }
}

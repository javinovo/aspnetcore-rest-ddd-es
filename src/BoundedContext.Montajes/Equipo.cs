using BoundedContext.Montajes.Events;
using Infrastructure.Domain;
using System;

namespace BoundedContext.Montajes
{
    public class Equipo : AggregateRoot
    {
        private Guid _id;
        public override Guid Id => _id;

        public string Nombre;

        public Equipo() { }
        public Equipo(Guid id, string nombre)
        {
            ApplyChange(new EquipoCreado(id, nombre));
        }
        void Apply(EquipoCreado e)
        {
            _id = e.Id;
            Nombre = e.Nombre;
        }

        public void ActualizarNombre(string nuevoNombre)
        {
            if (string.IsNullOrEmpty(nuevoNombre)) throw new ArgumentException(nameof(nuevoNombre));
            ApplyChange(new NombreEquipoActualizado(_id, nuevoNombre));
        }
        void Apply(NombreEquipoActualizado e) =>
            Nombre = e.NuevoNombre;        
    }
}

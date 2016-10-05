using System;

namespace ReadModel.Montajes.DTO
{
    public class EquipoDto
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public string Nombre { get; set; }

        public EquipoDto(Guid id, int version, string nombre)
        {
            Id = id;
            Version = version;
            Nombre = nombre;
        }

        public EquipoDto(BoundedContext.Montajes.Equipo aggregate)
            : this(aggregate.Id, aggregate.Version, aggregate.Nombre)
        { }
    }
}

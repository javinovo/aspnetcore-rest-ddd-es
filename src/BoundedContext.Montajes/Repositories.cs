using BoundedContext.Montajes.Events;
using Infrastructure.Domain;
using Infrastructure.Domain.Interfaces;

namespace BoundedContext.Montajes
{
    // Just a convenience class to hide the concrete types
    public class EquiposRepository : Repository<Equipo, EquipoCreado>
    {
        public EquiposRepository(IEventStore storage) : base(storage) { }
    }
}

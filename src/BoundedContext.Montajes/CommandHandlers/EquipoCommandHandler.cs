using BoundedContext.Montajes.Commands;
using Infrastructure.Domain;
using Infrastructure.Domain.Interfaces;

namespace BoundedContext.Montajes.CommandHandlers
{
    public class EquipoCommandHandler : IHandle<CrearEquipo>, IHandle<ActualizarNombreEquipo>
    {
        readonly IRepository<Equipo, Events.EquipoCreado> _repository;

        public EquipoCommandHandler(IRepository<Equipo, Events.EquipoCreado> repository)
        {
            _repository = repository;
        }

        public void Handle(CrearEquipo message)
        {
            var equipo = new Equipo(message.EquipoId, message.Nombre);
            _repository.Save(equipo, AggregateRoot.PreCreateVersion);
        }

        public void Handle(ActualizarNombreEquipo message)
        {
            var equipo = _repository.Find(message.EquipoId);
            equipo.ActualizarNombre(message.NuevoNombre);
            _repository.Save(equipo, message.OriginalVersion);
        }
    }
}

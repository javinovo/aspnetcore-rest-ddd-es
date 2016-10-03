using BoundedContext.Montajes.Commands;
using Infrastructure.Domain.Interfaces;

namespace BoundedContext.Montajes.CommandHandlers
{
    public class EquipoCommandHandler : IHandle<CrearEquipo>, IHandle<ActualizarNombreEquipo>
    {
        readonly IRepository<Equipo> _repository;

        public EquipoCommandHandler(IRepository<Equipo> repository)
        {
            _repository = repository;
        }

        public void Handle(CrearEquipo message)
        {
            var equipo = new Equipo(message.EquipoId, message.Nombre);
            _repository.Save(equipo, -1);
        }

        public void Handle(ActualizarNombreEquipo message)
        {
            var equipo = _repository.GetById(message.EquipoId);
            equipo.ActualizarNombre(message.NuevoNombre);
            _repository.Save(equipo, message.OriginalVersion);
        }
    }
}

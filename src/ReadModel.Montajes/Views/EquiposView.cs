using BoundedContext.Montajes.Events;
using Infrastructure.Domain.Interfaces;
using ReadModel.Montajes.DTO;
using System;
using System.Collections.Generic;

namespace ReadModel.Montajes.Views
{
    public class EquiposView : IHandle<EquipoCreado>, IHandle<NombreEquipoActualizado>
    {
        static Dictionary<Guid, EquipoDto> _dtos = new Dictionary<Guid, EquipoDto>();

        public EquiposView(IEnumerable<EquipoDto> snapshot)
        {
            foreach (var dto in snapshot)
                _dtos[dto.Id] = dto;
        }

        public void Handle(EquipoCreado message) =>
            _dtos[message.SourceId] = new EquipoDto(message.SourceId, message.Version, message.Nombre);

        public void Handle(NombreEquipoActualizado message)
        {
            _dtos[message.SourceId].Nombre = message.NuevoNombre;
            _dtos[message.SourceId].Version = message.Version;
        }

        // ToDo: ReadModelFacade?
        public static IEnumerable<EquipoDto> DTOs => _dtos.Values;

        public static EquipoDto Find(Guid id) => _dtos[id];
    }
}

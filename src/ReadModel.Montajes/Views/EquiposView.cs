using BoundedContext.Montajes.Events;
using Infrastructure.Domain.Interfaces;
using ReadModel.Montajes.DTO;
using System;
using System.Collections.Generic;

namespace ReadModel.Montajes.Views
{
    public class EquiposView : IHandle<EquipoCreado>, IHandle<NombreEquipoActualizado>
    {
        readonly Dictionary<Guid, EquipoDto> _dtos = new Dictionary<Guid, EquipoDto>();

        public EquiposView(IMessageBroker messageBroker)
        {
            messageBroker.RegisterHandler<EquipoCreado>(Handle);
            messageBroker.RegisterHandler<NombreEquipoActualizado>(Handle);
        }

        public void LoadSnapshot(IEnumerable<EquipoDto> snapshot)
        {
            _dtos.Clear();
            foreach (var dto in snapshot)
                _dtos[dto.Id] = dto;
        }

        // ToDo: ReadModelFacade?
        public IEnumerable<EquipoDto> FindAll() => _dtos.Values;

        public EquipoDto Find(Guid id) =>
            _dtos.ContainsKey(id) ? _dtos[id] : null;

        #region Event handlers

        public void Handle(EquipoCreado message) =>
            _dtos[message.SourceId] = new EquipoDto(message.SourceId, message.Version, message.Nombre);

        public void Handle(NombreEquipoActualizado message)
        {
            _dtos[message.SourceId].Nombre = message.NuevoNombre;
            _dtos[message.SourceId].Version = message.Version;
        }

        #endregion
    }
}

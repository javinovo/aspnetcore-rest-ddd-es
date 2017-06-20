using BoundedContext.Teams.Events;
using Infrastructure.Domain.Interfaces;
using ReadModel.Teams.DTO;
using System;
using System.Collections.Generic;

namespace ReadModel.Teams.Views
{
    public class TeamsView : IHandle<TeamCreated>, IHandle<TeamNameUpdated>, IHandle<TeamDissolved>
    {
        readonly Dictionary<Guid, TeamDto> _dtos = new Dictionary<Guid, TeamDto>();

        public TeamsView(IMessageBroker messageBroker)
        {
            messageBroker.RegisterHandler<TeamCreated>(Handle);
            messageBroker.RegisterHandler<TeamNameUpdated>(Handle);
            messageBroker.RegisterHandler<TeamDissolved>(Handle);
        }

        public void LoadSnapshot(IEnumerable<TeamDto> snapshot)
        {
            _dtos.Clear();
            foreach (var dto in snapshot)
                _dtos[dto.Id] = dto;
        }

        // ToDo: ReadModelFacade?
        public IEnumerable<TeamDto> FindAll() => _dtos.Values;

        public TeamDto Find(Guid id) =>
            _dtos.ContainsKey(id) ? _dtos[id] : null;

        #region Event handlers

        public void Handle(TeamCreated message) =>
            _dtos[message.SourceId] = new TeamDto(message.SourceId, message.Version, message.Name, message.IsActive);

        public void Handle(TeamNameUpdated message)
        {
            _dtos[message.SourceId].Name = message.NewName;
            _dtos[message.SourceId].Version = message.Version;
        }

        public void Handle(TeamDissolved message)
        {
            _dtos[message.SourceId].IsActive = false;
            _dtos[message.SourceId].Version = message.Version;
        }

        #endregion
    }
}

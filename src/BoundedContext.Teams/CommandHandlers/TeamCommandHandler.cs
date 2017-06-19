using BoundedContext.Teams.Commands;
using Infrastructure.Domain;
using Infrastructure.Domain.Interfaces;

namespace BoundedContext.Teams.CommandHandlers
{
    /// <summary>
    /// Application service in charge of processing the commands. Contains the infrastructure needed to execute the commands (ie. the repository)
    /// </summary>
    public class TeamCommandHandler : IHandle<CreateTeam>, IHandle<UpdateTeamName>
    {
        readonly IRepository<Team, Events.TeamCreated> _repository;

        public TeamCommandHandler(IMessageBroker messageBroker, IRepository<Team, Events.TeamCreated> repository)
        {
            _repository = repository;

            messageBroker.RegisterHandler<CreateTeam>(Handle);
            messageBroker.RegisterHandler<UpdateTeamName>(Handle);
        }

        public void Handle(CreateTeam message)
        {
            var team = new Team(message.TeamId, message.Name);
            _repository.Save(team, AggregateRoot.PreCreateVersion);
        }

        public void Handle(UpdateTeamName message)
        {
            var team = _repository.Find(message.TeamId);
            team.UpdateName(message.NewName);
            _repository.Save(team, message.OriginalVersion);
        }
    }
}

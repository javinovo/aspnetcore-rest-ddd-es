using BoundedContext.Teams.Events;
using Infrastructure.Domain;
using Infrastructure.Domain.Interfaces;

namespace BoundedContext.Teams.Repositories
{
    // Just a convenience class to hide the concrete types
    public class TeamRepository : Repository<Team, TeamCreated>
    {
        public TeamRepository(IEventStore storage) : base(storage) { }
    }
}

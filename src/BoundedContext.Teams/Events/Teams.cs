using Infrastructure.Domain;
using System;

namespace BoundedContext.Teams.Events
{
    public class TeamCreated : Event
    {
        public readonly string Name;

        public TeamCreated(Guid id, string name) : base(id)
        {
            Name = name;
        }
    }

    public class TeamNameUpdated : Event
    {
        public readonly string NewName;

        public TeamNameUpdated(Guid id, string newName) : base(id)
        {
            NewName = newName;
        }
    }
}

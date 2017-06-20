using Infrastructure.Domain;
using System;

namespace BoundedContext.Teams.Events
{
    public class TeamCreated : Event
    {
        public readonly string Name;
        public readonly bool IsActive;

        public TeamCreated(Guid id, string name, bool isActive) : base(id)
        {
            Name = name;
            IsActive = isActive;
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

    public class TeamDissolved : Event
    {
        public TeamDissolved(Guid id) : base(id) { }
    }
}

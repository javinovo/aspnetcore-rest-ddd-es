using Infrastructure.Domain.Interfaces;
using System;

namespace BoundedContext.Teams.Commands
{
    public class CreateTeam : ICommand
    {
        public readonly Guid TeamId;
        public readonly string Name;

        public CreateTeam(Guid id, string name)
        {
            TeamId = id;
            Name = name;
        }
    }

    public abstract class TeamCommand : ICommand
    {
        public readonly Guid TeamId;
        public readonly int OriginalVersion;

        public TeamCommand(Guid id, int originalVersion)
        {
            TeamId = id;
            OriginalVersion = originalVersion;
        }
    }

    public class UpdateTeamName : TeamCommand
    {
        public readonly string NewName;

        public UpdateTeamName(Guid id, string name, int originalVersion) : base(id, originalVersion)
        {
            NewName = name;
        }
    }

    public class DissolveTeam : TeamCommand
    {
        public DissolveTeam(Guid id, int originalVersion) : base(id, originalVersion)
        {
        }
    }
}

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

    public class UpdateTeamName : ICommand
    {
        public readonly Guid TeamId;
        public readonly string NewName;
        public readonly int OriginalVersion;

        public UpdateTeamName(Guid id, string name, int originalVersion)
        {
            TeamId = id;
            NewName = name;
            OriginalVersion = originalVersion;
        }
    }
}

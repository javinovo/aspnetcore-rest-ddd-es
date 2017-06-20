using System;

namespace ReadModel.Teams.DTO
{
    public class TeamDto
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

        public TeamDto(Guid id, int version, string name, bool isActive)
        {
            Id = id;
            Version = version;
            Name = name;
            IsActive = isActive;
        }

        public TeamDto(BoundedContext.Teams.Team aggregate)
            : this(aggregate.Id, aggregate.Version, aggregate.Name, aggregate.IsActive)
        { }
    }
}

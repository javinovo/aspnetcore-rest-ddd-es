using System;

namespace ReadModel.Teams.DTO
{
    public class TeamDto
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public string Name { get; set; }

        public TeamDto(Guid id, int version, string name)
        {
            Id = id;
            Version = version;
            Name = name;
        }

        public TeamDto(BoundedContext.Teams.Team aggregate)
            : this(aggregate.Id, aggregate.Version, aggregate.Name)
        { }
    }
}

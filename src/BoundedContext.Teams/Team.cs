﻿using BoundedContext.Teams.Events;
using Infrastructure.Domain;
using System;

namespace BoundedContext.Teams
{
    public class Team : AggregateRoot
    {
        private Guid _id;
        public override Guid Id => _id;

        public string Name;

        public Team() { }
        public Team(Guid id, string name)
        {
            ApplyChange(new TeamCreated(id, name));
        }
        void Apply(TeamCreated e)
        {
            _id = e.SourceId;
            Name = e.Name;
        }

        public void UpdateName(string newName)
        {
            if (string.IsNullOrEmpty(newName)) throw new ArgumentException(nameof(newName));
            ApplyChange(new TeamNameUpdated(_id, newName));
        }
        void Apply(TeamNameUpdated e) =>
            Name = e.NewName;        
    }
}

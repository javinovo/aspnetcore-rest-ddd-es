using System;
using System.Collections.Generic;

namespace Infrastructure.Domain
{
    public abstract class AggregateRoot
    {
        /// <summary>
        /// Initial value for Version in order for the first event applied to get a Version value of 0.
        /// It is also the first expected version value for <see cref="Repository{T, TCreatedEvent}.Save(AggregateRoot, int)"/> 
        /// </summary>
        public const int PreCreateVersion = -1;

        private readonly List<Event> _changes = new List<Event>();

        public abstract Guid Id { get; }
        public int Version { get; internal set; } = PreCreateVersion;

        public IEnumerable<Event> GetUncommittedChanges() => _changes;

        public void MarkChangesAsCommitted() => _changes.Clear();

        public void LoadsFromHistory(IEnumerable<Event> history)
        {
            foreach (var e in history) ApplyChange(e, false);
        }

        protected void ApplyChange(Event @event) => ApplyChange(@event, true);

        // push atomic aggregate changes to local history for further processing (EventStore.SaveEvents)
        private void ApplyChange(Event @event, bool isNew)
        {
            this.AsDynamic().Apply(@event);

            if (isNew) // New event: increment version, set up the event and add it to the uncommitted list
            {
                @event.Version = ++Version;
                _changes.Add(@event);
            }
            else // Rehydrating: update the current version from the event
            {
                Version = @event.Version;
            }
        }
    }
}

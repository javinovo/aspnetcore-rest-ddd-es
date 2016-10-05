using System;
using System.Collections.Generic;

namespace Infrastructure.Domain.Interfaces
{
    public interface IEventStore
    {
        void SaveEvents<T>(Guid aggregateId, IEnumerable<Event> events, int expectedVersion) where T : AggregateRoot;
        List<Event> GetEventsForAggregate<T>(Guid aggregateId) where T : AggregateRoot;
        List<T> GetEventsForType<T>(int startIndex, int maxCount) where T : Event;
    }
}

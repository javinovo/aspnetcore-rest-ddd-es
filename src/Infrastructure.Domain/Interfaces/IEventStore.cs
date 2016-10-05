﻿using System;
using System.Collections.Generic;

namespace Infrastructure.Domain.Interfaces
{
    public interface IEventStore
    {
        void SaveEvents(string aggregateType, Guid aggregateId, IEnumerable<Event> events, int expectedVersion);
        List<Event> GetEventsForAggregate(string aggregateType, Guid aggregateId);
        List<T> GetEventsForType<T>(int startIndex, int maxCount) where T : Event;
        //ToDo: List<Guid> EnumerateAggregates(string aggregateType, int startIndex, int maxCount);
    }
}

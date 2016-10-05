using Domain.Exceptions;
using Infrastructure.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Domain
{
    public class FakeEventStore : IEventStore
    {
        private readonly IEventPublisher _publisher;

        public FakeEventStore(IEventPublisher publisher)
        {
            _publisher = publisher;
        }

        private readonly Dictionary<Guid, List<Event>> _current = new Dictionary<Guid, List<Event>>();

        public void SaveEvents<T>(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
            where T : AggregateRoot
        {
            List<Event> aggregateEvents;

            // try to get event descriptors list for given aggregate id
            // otherwise -> create empty dictionary
            if (!_current.TryGetValue(aggregateId, out aggregateEvents))
            {
                aggregateEvents = new List<Event>();
                _current.Add(aggregateId, aggregateEvents);
            }
            // check whether latest event version matches current aggregate version
            // otherwise -> throw exception
            else if (aggregateEvents[aggregateEvents.Count - 1].Version != expectedVersion && expectedVersion != -1)
            {
                throw new ConcurrencyException();
            }

            foreach (var @event in events)
            {
                // push event to the event list for current aggregate
                aggregateEvents.Add(@event);

                // publish current event to the bus for further processing by subscribers
                _publisher.Publish(@event);
            }
        }

        // collect all processed events for given aggregate and return them as a list
        // used to build up an aggregate from its history (Domain.LoadsFromHistory)
        public List<Event> GetEventsForAggregate<T>(Guid aggregateId) where T : AggregateRoot
        {
            List<Event> aggregateEvents;

            if (!_current.TryGetValue(aggregateId, out aggregateEvents))
            {
                throw new AggregateNotFoundException();
            }

            return aggregateEvents.ToList();
        }

        public List<T> GetEventsForType<T>(int startIndex, int maxCount) where T : Event =>
            _current.Values.OfType<T>()
                .Skip(startIndex).Take(maxCount)
                .ToList();
    }
}

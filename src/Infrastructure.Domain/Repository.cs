using Infrastructure.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Domain
{
    public class Repository<T, TCreatedEvent> : IRepository<T, TCreatedEvent> 
        where T : AggregateRoot, new() //shortcut you can do as you see fit with new()
        where TCreatedEvent : Event
    {
        private readonly IEventStore _storage;

        public Repository(IEventStore storage)
        {
            _storage = storage;
        }

        public void Save(AggregateRoot aggregate, int expectedVersion)
        {
            _storage.SaveEvents<T>(aggregate.Id, aggregate.GetUncommittedChanges(), expectedVersion);
        }

        public T Find(Guid id, int version = AggregateRoot.PreCreateVersion)
        {
            var obj = new T();//lots of ways to do this
            var events = _storage.GetEventsForAggregate<T>(id);

            obj.LoadsFromHistory(
                version == AggregateRoot.PreCreateVersion  
                ? events
                : events.Where(ev => ev.Version <= version));

            return obj;
        }

        public IEnumerable<Guid> GetIds(int startIndex, int maxCount)  =>
            _storage.GetEventsForType<TCreatedEvent>(startIndex, maxCount).Select(x => x.SourceId);
    }
}

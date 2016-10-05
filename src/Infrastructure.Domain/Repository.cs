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
            _storage.SaveEvents(aggregate.GetType().FullName, aggregate.Id, aggregate.GetUncommittedChanges(), expectedVersion);
        }

        public T GetById(Guid id)
        {
            var obj = new T();//lots of ways to do this
            var e = _storage.GetEventsForAggregate(typeof(T).FullName, id);
            obj.LoadsFromHistory(e);
            return obj;
        }

        public IEnumerable<Guid> Enumerate(int startIndex, int maxCount)  =>
            _storage.GetEventsForType<TCreatedEvent>(startIndex, maxCount).Select(x => x.SourceId);
    }
}

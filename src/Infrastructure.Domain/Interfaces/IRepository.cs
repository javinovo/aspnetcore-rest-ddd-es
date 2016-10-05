using System;
using System.Collections.Generic;

namespace Infrastructure.Domain.Interfaces
{
    public interface IRepository<T, TCreatedEvent> 
        where T : AggregateRoot, new()
        where TCreatedEvent : Event
    {
        void Save(AggregateRoot aggregate, int expectedVersion);
        T GetById(Guid id);
        IEnumerable<Guid> Enumerate(int startIndex, int maxCount);
    }
}

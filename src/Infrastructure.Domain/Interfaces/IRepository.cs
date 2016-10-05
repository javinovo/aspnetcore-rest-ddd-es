using System;
using System.Collections.Generic;

namespace Infrastructure.Domain.Interfaces
{
    public interface IRepository<T, TCreatedEvent> 
        where T : AggregateRoot, new()
        where TCreatedEvent : Event
    {
        /// <param name="expectedVersion">Current version expected (the one stored prior to the save)</param>
        void Save(AggregateRoot aggregate, int expectedVersion);
        T Find(Guid id, int version = AggregateRoot.PreCreateVersion);
        IEnumerable<Guid> GetIds(int startIndex, int maxCount);
    }
}

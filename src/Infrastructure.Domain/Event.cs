using Infrastructure.Domain.Interfaces;
using System;

namespace Infrastructure.Domain
{
    public class Event : IMessage
    {
        public readonly Guid SourceId;
        public int Version { get; internal set; } // Updated by AggregateRoot.ApplyChange (otherwise it'd be readonly)

        public Event(Guid sourceId)
        {
            SourceId = sourceId;
        }
    }
}
using EventStore.ClientAPI;
using Infrastructure.Domain;
using Infrastructure.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventStoreFacade
{
    public class EventStore : IEventStore
    {
        readonly ConnectionSettings Settings = ConnectionSettings.Create()
            //.KeepReconnecting().KeepRetrying()
            .KeepReconnecting().LimitAttemptsForOperationTo(60 * 60 / 3).SetOperationTimeoutTo(TimeSpan.FromSeconds(3));
        readonly IEventStoreConnection Connection = null;
        readonly IEventPublisher _publisher;

        public EventStore(IEventPublisher publisher)
        {
            _publisher = publisher;

            try
            {
                Connection = EventStoreConnection.Create(Settings, new Uri("tcp://localhost:1113"));
                Connection.Disconnected += (s, e) =>
                {
                    //Serilog.Log.ForContext(typeof(EventStore))
                    //    .Warning("Connection.Disconnected: {@Args}", e);
                };
                Connection.Reconnecting += (s, e) =>
                {
                    //Serilog.Log.ForContext(typeof(EventStore))
                    //    .Warning("Connection.Reconnecting: {@Args}", e);
                };
                Connection.ErrorOccurred += (s, e) =>
                {
                    //Serilog.Log.ForContext(typeof(EventStore))
                    //    .Error(e.Exception, "Connection.ErrorOccurred: {@Args}");
                };

                Connection.ConnectAsync().Wait();
            }
            catch (Exception ex)
            {
                //Serilog.Log.ForContext(typeof(EventStore))
                //    .Error(ex, "Error reconnecting to EventStore at {ServerUri}", ServerUri);
            }
        }

        public List<Event> GetEventsForAggregate(string aggregateType, Guid aggregateId)
        {
            var streamEvents = new List<ResolvedEvent>();

            StreamEventsSlice currentSlice;
            var nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice = Connection.ReadStreamEventsForwardAsync(Utils.GetAggregateStreamName(aggregateType, aggregateId), nextSliceStart, 200, true).Result;
                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);
            } while (!currentSlice.IsEndOfStream);

            var deserializedEvents = streamEvents.Select(ev => Utils.FromJson(Utils.Encoding.GetString(ev.Event.Data), ev.Event.EventType))
                .Cast<Event>().ToList();

            return deserializedEvents;
        }

        public void SaveEvents(string aggregateType, Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
        {
            var i = expectedVersion;

            // iterate through current aggregate events increasing version with each processed event
            foreach (var @event in events)
                @event.Version = ++i;

            var result = Connection.AppendToStreamAsync(Utils.GetAggregateStreamName(aggregateType, aggregateId), expectedVersion,            
                events.Select(x => Utils.CreateEvent(x.GetType().FullName, x))).Result;

            foreach (var @event in events)
                // publish current event to the bus for further processing by subscribers
                _publisher.Publish(@event);
        }
    }
}

using Domain.Exceptions;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStoreFacade.Serialization;
using Infrastructure.Domain;
using Infrastructure.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventStoreFacade
{
    public class EventStore : IEventStore
    {
        const string DefaultEventAssembliesPrefix = "BoundedContext.";
        const string DefaultServerUri = "tcp://localhost:1113";
        const int MaxPageSize = 4096; // EventStore.ClientAPI.Consts.MaxReadSize (has internal visibility so we copy the value)

        readonly ConnectionSettings Settings = ConnectionSettings.Create()           
            .KeepReconnecting().LimitAttemptsForOperationTo(60 * 60 / 3)
            .SetOperationTimeoutTo(TimeSpan.FromSeconds(3));
        readonly IEventStoreConnection Connection = null;

        readonly IEventPublisher _publisher;
        readonly EventSerializer _eventSerializer;
            

        public EventStore(ILogger<EventStore> logger, IEventPublisher publisher, IOptions<EventStoreOptions> options)
        {
            _publisher = publisher;

            var assemblyNameFilter = options.Value.EventAssembliesPrefix ?? DefaultEventAssembliesPrefix;
            _eventSerializer = new EventSerializer(
                new DependencyContextFinder(assemblyNameFilter)); // Preferred since it doesn't require a path but will be incompatible with .NET Standard 2.0
                //new RuntimeLoaderFinder(new System.IO.DirectoryInfo(@".\bin\Debug\netcoreapp1.0"), assemblyNameFilter));

            try
            {
                Connection = EventStoreConnection.Create(Settings, new Uri(options.Value.ServerUri ?? DefaultServerUri));

                Connection.Disconnected += (s, e) =>
                    logger.LogError("EventStore Disconnected");

                Connection.Reconnecting += (s, e) =>
                    logger.LogError("EventStore Reconnecting");

                Connection.ErrorOccurred += (s, e) =>
                    logger.LogError("EventStore ErrorOccurred");

                Connection.ConnectAsync().Wait();
            }
            catch (Exception ex)
            {
                logger.LogError(0, ex, "EventStore Exception");
            }
        }

        public void SaveEvents<T>(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
            where T : AggregateRoot
        {
            try
            {
                var _ = Connection.AppendToStreamAsync(GetAggregateStreamName(typeof(T).FullName, aggregateId), expectedVersion,
                    events.Select(x => _eventSerializer.CreateEvent(x.GetType().FullName, x))).Result;
            }
            catch (AggregateException ae) when (ae.InnerException is WrongExpectedVersionException)
            {
                throw new ConcurrencyException();
            }

            foreach (var @event in events)
                // publish current event to the bus for further processing by subscribers
                _publisher.Publish(@event);
        }


        public List<Event> GetEventsForAggregate<T>(Guid aggregateId) where T : AggregateRoot
        {
            var events = ReadStream(GetAggregateStreamName(typeof(T).FullName, aggregateId))
                .Select(ev => _eventSerializer.FromData(ev.Event.Data, ev.Event.EventType))
                .Cast<Event>().ToList();

            if (events.Count == 0)
                throw new AggregateNotFoundException();

            return events;
        }

        public List<T> GetEventsForType<T>(int startIndex, int maxCount) 
            where T : Event =>

            ReadStream(GetEventTypeStreamName(typeof(T).FullName))
                .Select(ev => _eventSerializer.FromData<T>(ev.Event.Data))
                .Skip(startIndex).Take(maxCount)
                .ToList();

        // ToDo: support paging
        List<ResolvedEvent> ReadStream(string streamName) 
        {
            var streamEvents = new List<ResolvedEvent>();

            StreamEventsSlice currentSlice;
            var nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice = Connection.ReadStreamEventsForwardAsync(streamName, nextSliceStart, MaxPageSize, true).Result;
                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);
            } while (!currentSlice.IsEndOfStream);

            return streamEvents;
        }

        #region Stream mapping

        /// <summary>
        /// Stream containing all the events of a given type (requires built-in projections enabled)
        /// </summary>
        static string GetEventTypeStreamName(string eventName) => $"$et-{eventName}";

        /// <summary>
        /// Stream containing all the events of a given aggregate instance
        /// </summary>
        static string GetAggregateStreamName(string aggregateName, Guid aggregateId) => $"{aggregateName}-{aggregateId}";

        #endregion
    }
}

using EventStore.ClientAPI;
using Newtonsoft.Json;
using System;
using System.Text;

namespace EventStoreFacade
{
    public static class Utils
    {
        private static readonly Encoding Encoding = Encoding.UTF8;

        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        public static string GetEventTypeStreamName(string eventFullName) =>
            $"$et-{eventFullName}";

        public static string GetAggregateStreamName(string aggregateFullName, Guid aggregateId) =>
            $"{aggregateFullName}-{aggregateId}";

        public static EventData CreateEvent(string eventType, object data, object metadata = null)
        {
            var json = JsonConvert.SerializeObject(data, SerializerSettings);

            return new EventData(Guid.NewGuid(), eventType, true,
                Encoding.GetBytes(json),
                metadata != null ? Encoding.GetBytes(JsonConvert.SerializeObject(metadata, SerializerSettings)) : null);
        }

        public static object FromJson(string json, Type type) =>
            JsonConvert.DeserializeObject(json, type, SerializerSettings);
    }
}

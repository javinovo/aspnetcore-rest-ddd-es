using EventStore.ClientAPI;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;

namespace EventStoreFacade
{
    public static class Utils
    {
        public static readonly Encoding Encoding = Encoding.UTF8;
        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        public static string GetEventTypeStreamName(string eventName) => $"$et-{eventName}";

        public static string GetAggregateStreamName(string aggregateName, Guid aggregateId) => $"{aggregateName}-{aggregateId}";

        public static EventData CreateEvent(string eventType, object data, object metadata = null)
        {
            var json = JsonConvert.SerializeObject(data, SerializerSettings);

            var eventId = Guid.NewGuid(); // (data as Event)?.MessageId?

            return new EventData(eventId, eventType, true,
                Encoding.GetBytes(json),
                metadata != null ? Encoding.GetBytes(JsonConvert.SerializeObject(metadata, SerializerSettings)) : null);
        }

        public static object FromJson(string json, string typeFullName)
        {
            Type type = Type.GetType(typeFullName);

            if (type == null)
            {
                var namespaceStart = typeFullName.Split('.').First();

                var types =
                    (from lib in Microsoft.Extensions.DependencyModel.DependencyContext.Default.RuntimeLibraries
                     where lib.Name.StartsWith(namespaceStart)
                     select System.Reflection.Assembly.Load(new System.Reflection.AssemblyName(lib.Name)))
                     .SelectMany(x => x.ExportedTypes);

                type = types.Single(t => t.FullName.Equals(typeFullName, StringComparison.OrdinalIgnoreCase));
            }

            return JsonConvert.DeserializeObject(json, type, SerializerSettings);
        }
    }
}

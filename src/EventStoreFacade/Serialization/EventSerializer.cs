using EventStore.ClientAPI;
using Newtonsoft.Json;
using System;
using System.Text;

namespace EventStoreFacade.Serialization
{
    public class EventSerializer
    {
        readonly ITypeFinder _typeFinder;
        readonly Encoding Encoding = Encoding.UTF8;
        readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new WritableNonPublicsContractResolver()
        };

        public EventSerializer(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }

        public EventData CreateEvent(string eventType, object data, object metadata = null)
        {
            var json = JsonConvert.SerializeObject(data, SerializerSettings);

            var eventId = Guid.NewGuid(); // (data as Event)?.MessageId?

            return new EventData(eventId, eventType, true,
                Encoding.GetBytes(json),
                metadata != null ? Encoding.GetBytes(JsonConvert.SerializeObject(metadata, SerializerSettings)) : null);
        }

        object FromJson(string json, Type type) => JsonConvert.DeserializeObject(json, type, SerializerSettings);

        object FromData(byte[] data, Type type) => FromJson(Encoding.GetString(data), type);

        public TEvent FromData<TEvent>(byte[] data) => (TEvent)FromData(data, typeof(TEvent));

        public object FromData(byte[] data, string typeFullName)
        {
            Type type = Type.GetType(typeFullName) ?? _typeFinder.Find(typeFullName);

            if (type == null)
                throw new TypeLoadException($"Type not found: {typeFullName}");

            return FromData(data, type);
        }
    }
}

using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EventStoreFacade
{
    public static class Utils
    {
        public static readonly Encoding Encoding = Encoding.UTF8;
        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new WritableNonPublicsContractResolver()
        };

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

        public static object FromJson(string json, Type type) => JsonConvert.DeserializeObject(json, type, SerializerSettings);

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

            return FromJson(json, type);
        }
    }

    /// <summary>
    /// Makes non-public properties and fields writable in order for the serializer to be able to set their values when converting from JSON
    /// http://stackoverflow.com/a/6602715/3042204
    /// </summary>
    public class WritableNonPublicsContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.Writable = CanSetMemberValue(member, true);
            return property;
        }

        public static bool CanSetMemberValue(MemberInfo member, bool nonPublic)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    var fieldInfo = (FieldInfo)member;

                    return nonPublic || fieldInfo.IsPublic;
                case MemberTypes.Property:
                    var propertyInfo = (PropertyInfo)member;

                    if (!propertyInfo.CanWrite)
                        return false;
                    if (nonPublic)
                        return true;
                    return (propertyInfo.GetSetMethod(nonPublic) != null);
                default:
                    return false;
            }
        }
    }
}

namespace EventStoreFacade
{
    public class EventStoreOptions
    {
        /// <summary>
        /// EventStore server URI
        /// </summary>
        public string ServerUri { get; set; }

        /// <summary>
        /// Prefix used to filter the assembly files containing event types
        /// </summary>
        public string EventAssembliesPrefix { get; set; }
    }
}

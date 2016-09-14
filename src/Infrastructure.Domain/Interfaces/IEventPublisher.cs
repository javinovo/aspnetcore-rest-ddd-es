namespace Infrastructure.Domain.Interfaces
{
    public interface IEventPublisher
    {
        void Publish<T>(T @event) where T : Event;
    }
}

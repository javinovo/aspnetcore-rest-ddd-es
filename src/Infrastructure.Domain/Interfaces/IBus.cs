namespace Infrastructure.Domain.Interfaces
{
    public interface IBus : ICommandSender, IEventPublisher
    {
    }
}

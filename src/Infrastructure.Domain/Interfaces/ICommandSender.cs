namespace Infrastructure.Domain.Interfaces
{
    public interface ICommandSender
    {
        void Send<T>(T command) where T : ICommand;
    }
}

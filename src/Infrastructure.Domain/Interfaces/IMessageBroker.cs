using System;

namespace Infrastructure.Domain.Interfaces
{
    public interface IMessageBroker
    {
        void RegisterHandler<T>(Action<T> handler) where T : IMessage;
    }
}

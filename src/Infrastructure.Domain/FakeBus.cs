using Infrastructure.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Infrastructure.Domain
{
    public class FakeBus : IMessageBroker, ICommandSender, IEventPublisher
    {
        readonly Dictionary<Type, List<Action<IMessage>>> _routes = new Dictionary<Type, List<Action<IMessage>>>();

        public void RegisterHandler<T>(Action<T> handler) where T : IMessage
        {
            if (!_routes.TryGetValue(typeof(T), out var handlers))
            {
                handlers = new List<Action<IMessage>>();
                _routes.Add(typeof(T), handlers);
            }

            handlers.Add((x => handler((T)x)));
        }

        public void Send<T>(T command) where T : ICommand
        {
            if (_routes.TryGetValue(typeof(T), out var handlers))
            {
                if (handlers.Count != 1) throw new InvalidOperationException("cannot send to more than one handler");
                handlers[0](command);
            }
            else
                throw new InvalidOperationException("no handler registered");
        }

        public void Publish<T>(T @event) where T : Event
        {
            if (_routes.TryGetValue(@event.GetType(), out var handlers))
                foreach (var handler in handlers)
                {
                    //dispatch on thread pool for added awesomeness
                    var handler1 = handler;
                    System.Threading.ThreadPool.QueueUserWorkItem(x => handler1(@event));
                }
        }
    }
}

using Infrastructure.Domain.Interfaces;

namespace Infrastructure.Domain
{
    public class Event : IMessage
    {
        public int Version;
    }
}
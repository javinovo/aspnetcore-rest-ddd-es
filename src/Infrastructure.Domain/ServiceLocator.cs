namespace Infrastructure.Domain
{
    public static class ServiceLocator
    {
        private static FakeBus _bus = new FakeBus();
        public static FakeBus Bus => _bus;
    }
}

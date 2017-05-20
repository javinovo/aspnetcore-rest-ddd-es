using BoundedContext.Montajes.CommandHandlers;
using BoundedContext.Montajes.Repositories;
using Infrastructure.Domain;
using Infrastructure.Domain.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReadModel.Montajes.Views;
using System.Linq;

namespace WebApp
{
    static class ConfigurationExtensions
    {
        /// <summary>
        /// Configure Dependency Injections
        /// </summary>
        public static void ConfigureServices(this IServiceCollection services)
        {
            // Add our bus, write model repository and read model to Dependency Injection

            var bus = new FakeBus();
            services.AddSingleton<ICommandSender>(_ => bus);
            services.AddSingleton<IEventPublisher>(_ => bus);
            services.AddSingleton<IMessageBroker>(_ => bus);

            services.AddSingleton<IEventStore>(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger>();
                var eventPublisher = serviceProvider.GetService<IEventPublisher>();

                var eventStoreOptions = serviceProvider.GetService<IOptions<EventStoreFacade.EventStoreOptions>>();

                // We fall back to an in-memory fake event store if the real one wasn't configured in appsettings.json
                if (string.IsNullOrWhiteSpace(eventStoreOptions.Value.ServerUri))
                {
                    logger.LogInformation($"EventStore not configured, reverting to in-memory fake event store");
                    
                    return new FakeEventStore(eventPublisher);
                }
                else
                {
                    logger.LogInformation($"EventStore configured at: {eventStoreOptions.Value.ServerUri}");

                    return new EventStoreFacade.EventStore(
                        serviceProvider.GetService<ILogger<EventStoreFacade.EventStore>>(),
                        eventPublisher,
                        eventStoreOptions);
                }
            });

            services.AddSingleton(serviceProvider =>
                new EquiposRepository(serviceProvider.GetService<IEventStore>()));

            services.AddSingleton(serviceProvider => // Write model
                new EquipoCommandHandler(
                    serviceProvider.GetService<IMessageBroker>(),
                    serviceProvider.GetService<EquiposRepository>()));

            services.AddSingleton(serviceProvider => // Read model
                new EquiposView(serviceProvider.GetService<IMessageBroker>()));
        }

        /// <summary>
        /// Configure read and write models
        /// </summary>
        public static void ConfigureModels(this IApplicationBuilder app, EquiposRepository repo, EquiposView equiposView)
        {
            // Read models (Queries) setup

            var snapshot = // Current data snapshot: we retrieve all the aggregates from the repository
                repo.GetIds(0, int.MaxValue).ToArray()
                .Select(id => repo.Find(id))
                .Select(x => new ReadModel.Montajes.DTO.EquipoDto(x));

            equiposView.LoadSnapshot(snapshot);


            // Write models (Commands) setup

            // Nothing to do (handler registration is done in the EquipoCommandHandler constructor)
        }
    }
}

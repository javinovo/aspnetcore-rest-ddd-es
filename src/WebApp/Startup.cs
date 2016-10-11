using BoundedContext.Montajes.Commands;
using BoundedContext.Montajes.Events;
using BoundedContext.Montajes.Repositories;
using Infrastructure.Domain;
using Infrastructure.Domain.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
	
	public class Startup
	{
        IConfiguration _configuration;

        public Startup(IHostingEnvironment env)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

		public void ConfigureServices(IServiceCollection services)
		{
			// Add framework services

			services.AddMvc();
			services.AddLogging();

            services.AddOptions();           
            services.Configure<EventStoreFacade.EventStoreOptions>(_configuration.GetSection(nameof(EventStoreFacade.EventStoreOptions))); // Make IOptions<EventStoreFacade.EventStoreOptions> available through DI

            // Add our bus and repository

            var bus = new FakeBus();
            services.AddSingleton<ICommandSender>(_ => bus);
            services.AddSingleton<IEventPublisher>(_ => bus);
            services.AddSingleton<IMessageBroker>(_ => bus);

            services.AddSingleton<IEventStore>(serviceProvider =>
            {
                var eventStoreOptions = serviceProvider.GetService<IOptions<EventStoreFacade.EventStoreOptions>>();

                // We fall back to an in-memory fake event store if the real one wasn't configured in appsettings.json
                if (string.IsNullOrWhiteSpace(eventStoreOptions.Value.ServerUri))
                    return new FakeEventStore(serviceProvider.GetService<IEventPublisher>());
                else
                    return new EventStoreFacade.EventStore(
                        serviceProvider.GetService<ILogger<EventStoreFacade.EventStore>>(),
                        serviceProvider.GetService<IEventPublisher>(),
                        eventStoreOptions);
            });
            services.AddSingleton(serviceProvider =>
                new EquiposRepository(serviceProvider.GetService<IEventStore>()));
        }
	
		public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory,
            IMessageBroker messageBroker, EquiposRepository repo)
		{
#if DEBUG
            loggerFactory.AddDebug();
#endif
            loggerFactory.AddConsole();

            app.UseMvcWithDefaultRoute();

            ConfigureModels(messageBroker, repo);
		}

        void ConfigureModels(IMessageBroker messageBroker, EquiposRepository repo)
        {
            // Write models (Commands)

            var cmdHandler = new BoundedContext.Montajes.CommandHandlers.EquipoCommandHandler(repo);
            messageBroker.RegisterHandler<CrearEquipo>(cmdHandler.Handle);
            messageBroker.RegisterHandler<ActualizarNombreEquipo>(cmdHandler.Handle);

            // Read models (Queries)

            var snapshot = // Current data snapshot: we retrieve all the aggregates from the repository
                repo.GetIds(0, int.MaxValue).ToArray()
                .Select(id => repo.Find(id))
                .Select(x => new ReadModel.Montajes.DTO.EquipoDto(x));

            var equipoView = new ReadModel.Montajes.Views.EquiposView(snapshot);
            messageBroker.RegisterHandler<EquipoCreado>(equipoView.Handle);
            messageBroker.RegisterHandler<NombreEquipoActualizado>(equipoView.Handle);
        }
	}
}

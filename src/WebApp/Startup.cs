using BoundedContext.Montajes.Commands;
using BoundedContext.Montajes.Events;
using Infrastructure.Domain;
using Infrastructure.Domain.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
		public void ConfigureServices(IServiceCollection services)
		{
			// Add framework services.
			services.AddMvc();

			services.AddLogging();

            // Add our repository type
            ConfigureBus();
            services.AddSingleton<IBus, FakeBus>(_ => ServiceLocator.Bus);
		}
	
		public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
		{
#if DEBUG
            loggerFactory.AddDebug();
#endif
            app.UseMvcWithDefaultRoute();
		}

        void ConfigureBus()
        {
            var bus = ServiceLocator.Bus;

            var repo = new BoundedContext.Montajes.EquiposRepository(
                new EventStoreFacade.EventStore(bus));
                //new FakeEventStore(bus));

            // Write models (Commands)

            var cmdHandler = new BoundedContext.Montajes.CommandHandlers.EquipoCommandHandler(repo);
            bus.RegisterHandler<CrearEquipo>(cmdHandler.Handle);
            bus.RegisterHandler<ActualizarNombreEquipo>(cmdHandler.Handle);

            // Read models (Queries)

            var snapshot = // Current data snapshot: we retrieve all the aggregates from the repository
                repo.Enumerate(0, int.MaxValue).ToArray()
                .Select(id => repo.GetById(id))
                .Select(x => new ReadModel.Montajes.DTO.EquipoDto(x.Id, x.Version, x.Nombre));

            var equipoView = new ReadModel.Montajes.Views.EquiposView(snapshot);
            bus.RegisterHandler<EquipoCreado>(equipoView.Handle);
            bus.RegisterHandler<NombreEquipoActualizado>(equipoView.Handle);
        }
	}
}

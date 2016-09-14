using BoundedContext.Montajes.Commands;
using BoundedContext.Montajes.Events;
using Infrastructure.Domain;
using Infrastructure.Domain.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            var repo = new Repository<BoundedContext.Montajes.Equipo>(                    
                new Infrastructure.Domain.EventStore(bus));

            var cmdHandler = new BoundedContext.Montajes.CommandHandlers.EquipoCommandHandler(repo);
            bus.RegisterHandler<CrearEquipo>(cmdHandler.Handle);
            bus.RegisterHandler<ActualizarNombreEquipo>(cmdHandler.Handle);

            var equipoView = new ReadModel.Montajes.Views.EquiposView();
            bus.RegisterHandler<EquipoCreado>(equipoView.Handle);
            bus.RegisterHandler<NombreEquipoActualizado>(equipoView.Handle);
        }
	}
}

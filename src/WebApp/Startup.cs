using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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

            services.ConfigureServices();
        }
	
		public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory,
            BoundedContext.Montajes.Repositories.EquiposRepository repo,
            ReadModel.Montajes.Views.EquiposView view)
		{
#if DEBUG
            loggerFactory.AddDebug();
#endif
            loggerFactory.AddConsole();

            app.UseMvcWithDefaultRoute();

            app.ConfigureModels(repo, view);
		}
	}
}

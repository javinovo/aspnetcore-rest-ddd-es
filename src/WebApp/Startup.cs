using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args) =>
            new WebHostBuilder()            
                .UseKestrel()
                .UseStartup<Startup>()
                .Build()
                .Run();
    }
	
	public class Startup
	{
        IHostingEnvironment _environment;
        IConfiguration _configuration;

        public Startup(IHostingEnvironment env)
        {
            _environment = env;
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
            services.AddMvc()
                .AddJsonOptions(options =>
                {                    
                    if (_environment.IsDevelopment())
                        // SwaggerGen consumes MvcJsonOptions.SerializerSettings
                        options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                });
            services.AddLogging();

            // Inject an implementation of ISwaggerProvider with defaulted settings applied
            services.AddSwaggerGen();

            services.AddOptions()
                .Configure<EventStoreFacade.EventStoreOptions>(_configuration.GetSection(nameof(EventStoreFacade.EventStoreOptions))); // Make IOptions<EventStoreFacade.EventStoreOptions> available through DI

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

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();

            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUi();

            app.ConfigureModels(repo, view);
		}
	}
}

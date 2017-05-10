using Halcyon.Web.HAL.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;

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
            services.AddMvc(options =>                    
                    options.OutputFormatters.Add(new JsonHalOutputFormatter(
                        new[] { "application/hal+json", "application/vnd.example.hal+json", "application/vnd.example.hal.v1+json" })))
                .AddJsonOptions(options =>
                {                    
                    if (_environment.IsDevelopment())
                        // SwaggerGen consumes MvcJsonOptions.SerializerSettings
                        options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                });
            services.AddLogging();

            // Inject an implementation of ISwaggerProvider, enabling the inclusion XML comments
            services.AddSwaggerGen(options =>
                options.IncludeXmlComments(GetXmlCommentsFilePath()));

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
            if (_environment.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseSwagger();   // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwaggerUi(); // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)

            app.ConfigureModels(repo, view);
		}

        string GetXmlCommentsFilePath()
        {
            var app = PlatformServices.Default.Application;
            return System.IO.Path.Combine(app.ApplicationBasePath, System.IO.Path.ChangeExtension(app.ApplicationName, "xml"));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApplication
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
			services.AddSingleton<ITodoRepository, TodoRepository>();
		}
		
		public void Configure(IApplicationBuilder app)
		{
			/*
            app.Run(context =>
            {
                return context.Response.WriteAsync("Hello from ASP.NET Core!");
            });			
			*/
			app.UseMvcWithDefaultRoute();			
		}
	}
	
    public class TodoItem
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }
    }

    public interface ITodoRepository
    {
        void Add(TodoItem item);
        IEnumerable<TodoItem> GetAll();
        TodoItem Find(string key);
        TodoItem Remove(string key);
        void Update(TodoItem item);
    }
	
    public class TodoRepository : ITodoRepository
    {
        private static ConcurrentDictionary<string, TodoItem> _todos = 
              new ConcurrentDictionary<string, TodoItem>();

        public TodoRepository()
        {
            Add(new TodoItem { Name = "Item1" });
        }

        public IEnumerable<TodoItem> GetAll()
        {
            return _todos.Values;
        }

        public void Add(TodoItem item)
        {
            item.Key = Guid.NewGuid().ToString();
            _todos[item.Key] = item;
        }

        public TodoItem Find(string key)
        {
            TodoItem item;
            _todos.TryGetValue(key, out item);
            return item;
        }

        public TodoItem Remove(string key)
        {
            TodoItem item;
            _todos.TryGetValue(key, out item);
            _todos.TryRemove(key, out item);
            return item;
        }

        public void Update(TodoItem item)
        {
            _todos[item.Key] = item;
        }
    }	
	
	[Route("api/[controller]")]
    public class TodoController : Controller
    {
        public TodoController(ITodoRepository todoItems)
        {
            TodoItems = todoItems;
        }
        public ITodoRepository TodoItems { get; set; }
		
		public IEnumerable<TodoItem> GetAll()
		{
			return TodoItems.GetAll();
		}

		[HttpGet("{id}", Name = "GetTodo")]
		public IActionResult GetById(string id)
		{
			var item = TodoItems.Find(id);
			if (item == null)
			{
				return NotFound();
			}
			return new ObjectResult(item);
		}
		
		[HttpPost]
		public IActionResult Create([FromBody] TodoItem item)
		{
			if (item == null)
			{
				return BadRequest();
			}
			TodoItems.Add(item);
			return CreatedAtRoute("GetTodo", new { controller = "Todo", id = item.Key }, item);
		}		
		
		[HttpPut("{id}")]
		public IActionResult Update(string id, [FromBody] TodoItem item)
		{
			if (item == null || item.Key != id)
			{
				return BadRequest();
			}

			var todo = TodoItems.Find(id);
			if (todo == null)
			{
				return NotFound();
			}

			TodoItems.Update(item);
			return new NoContentResult();
		}		

		[HttpDelete("{id}")]
		public void Delete(string id)
		{
			TodoItems.Remove(id);
		}
    }
}

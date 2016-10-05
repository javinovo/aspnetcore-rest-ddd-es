# ASP.NET Core REST API for DDD Event Sourced to EventStore

Setup
-----

Edit `src\WebApp\Startup.cs` and pick the desired event store: `FakeEventStore` for in-memory or `EventStore` for [GES](https://geteventstore.com):

```
services.AddSingleton<IEventStore>(serviceProvider =>
	//new FakeEventStore(serviceProvider.GetService<IEventPublisher>()));
	new EventStoreFacade.EventStore(
		serviceProvider.GetService<ILogger<EventStoreFacade.EventStore>>(),
		serviceProvider.GetService<IEventPublisher>()));
```

Run:

```
cd src\WebApp
dotnet restore
dotnet run
```

Projects
--------

* Reads
   * ReadModel.Montajes: read model serving the queries. Loads a fake snapshot on start (state restored from the event store) and then reacts to domain events to update its own model. Queries are replied using this model.
* Writes
   * BoundedContext.Montajes: domain and command processing.
   * EventStoreFacade: event storage to [GES](https://geteventstore.com):
* Infrastructure.Domain: DDD, CQRS, and Event Sourcing infrastructure. Originated from [Greg Young's simple CQRS example](https://github.com/gregoryyoung/m-r).
* WebApp: web API.

Endpoints
---------

Example:

1. Create

  ```
  curl -X POST -H "Content-Type: application/json" -H "Cache-Control: no-cache" -d '{
	  Nombre: "prueba"
  }' "http://localhost:5000/api/EquipoMontaje/63931ea8-3f83-487c-8f21-01577a5157f9"
  ```

2. Update 

  ```
  curl -X PUT -H "Content-Type: application/json" -H "Cache-Control: no-cache" -d '{
	  NuevoNombre: "actualizado!!",
	  OriginalVersion: 0
  }' "http://localhost:5000/api/EquipoMontaje/63931ea8-3f83-487c-8f21-01577a5157f9/nombre"
  ```

3. Retrieve all

  ```
  curl -X GET -H "Cache-Control: no-cache" "http://localhost:5000/api/EquipoMontaje"
  ```
  
4. Retrieve one

  ```
  curl -X GET -H "Cache-Control: no-cache" "http://localhost:5000/api/EquipoMontaje/63931ea8-3f83-487c-8f21-01577a5157f9"
  ```
  
5. Retrieve a previous version

  ```
  curl -X GET -H "Cache-Control: no-cache" "http://localhost:5000/api/EquipoMontaje/63931ea8-3f83-487c-8f21-01577a5157f9/0"
  ```

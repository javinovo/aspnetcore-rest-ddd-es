# ASP.NET Core REST API for DDD Event Sourced to EventStore

Setup
-----

Optionally configure a [GES EventStore](https://geteventstore.com) by creating a `appsettings.json` file in the output directory (ie. `src\WebApp\bin\Debug\netcoreapp1.0`):

```
{
	"EventStoreOptions": {
		"ServerUri": "tcp://localhost:1113"
	}
}
```

Event type projections (`$et-` streams) are used so [projections must be enabled](http://docs.geteventstore.com/introduction/latest/setup_projections/).

If no `ServerUri` is provided a fake in-memory EventStore is used. This can be tried by omitting the `appsettings.json` file altogether or simply commenting the `ServerUri` line by prepending `//` to it.

Run:

```
cd src\WebApp
dotnet restore
dotnet run
```

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

Projects
--------

* Reads
   * ReadModel.Montajes: read model serving the queries. Loads a fake snapshot on start (state restored from the event store) and then reacts to domain events to update its own model. Queries are replied using this model.
* Writes
   * BoundedContext.Montajes: domain and command processing.
   * EventStoreFacade: event storage to [GES](https://geteventstore.com):
* Infrastructure.Domain: DDD, CQRS, and Event Sourcing infrastructure. Originated from [Greg Young's simple CQRS example](https://github.com/gregoryyoung/m-r).
* WebApp: web API.

Main components
---------------

* Messaging
	* IMessage
	* ICommand: command request message
	* Event: published event message
	* IHandle<IMessage>: message type handler
* Bus
	* IMessageBroker: manage message handlers
	* IEventPublisher: event publishing
	* ICommandSender: command requests
* Storage
	* IRepository<AggregateRoot, Event>: enumerate, find and save aggregates
	* IEventStore: load and save events
	
Bootstrapping
-------------

Takes place in `WebApp.Startup`:

* At `ConfigureServices` dependency injection is configured with instances of `ICommandSender`, `IEventPublisher`, `IMessageBroker`, and `IEventStore`, as well the different aggregate roots repositories.
* At `Configure` the read and write models are configured by registering the proper command and event handlers, and populating the read views with the initital data.

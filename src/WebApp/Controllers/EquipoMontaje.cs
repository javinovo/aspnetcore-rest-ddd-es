using BoundedContext.Montajes.Repositories;
using Domain.Exceptions;
using Infrastructure.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReadModel.Montajes.DTO;
using ReadModel.Montajes.Views;
using System;
using System.Collections.Generic;
using WebApp.Models;
using commands = BoundedContext.Montajes.Commands;

/*
curl -X GET -H "Cache-Control: no-cache" "http://localhost:5000/api/EquipoMontaje"

curl -X POST -H "Content-Type: application/json" -H "Cache-Control: no-cache" -d '{
	Nombre: "prueba"
}' "http://localhost:5000/api/EquipoMontaje/63931ea8-3f83-487c-8f21-01577a5157f9"

curl -X PUT -H "Content-Type: application/json" -H "Cache-Control: no-cache" -d '{
	NuevoNombre: "actualizado!!",
	OriginalVersion: 0
}' "http://localhost:5000/api/EquipoMontaje/63931ea8-3f83-487c-8f21-01577a5157f9/nombre"

curl -X GET -H "Cache-Control: no-cache" "http://localhost:5000/api/EquipoMontaje/63931ea8-3f83-487c-8f21-01577a5157f9"

curl -X GET -H "Cache-Control: no-cache" "http://localhost:5000/api/EquipoMontaje/63931ea8-3f83-487c-8f21-01577a5157f9/0"
 */

namespace WebApp.Controllers
{
    /// <summary>
    /// Facade exposed as a REST API which translates to queries and commands
    /// </summary>
    [Route("api/[controller]")]
    public class EquipoMontaje : Controller
    {
        ICommandSender _bus;
        ILogger<EquipoMontaje> _logger;
        EquiposView _readModelView;
        EquiposRepository _writeModelRepository;

        public EquipoMontaje(ILogger<EquipoMontaje> logger, ICommandSender bus, EquiposRepository repository, EquiposView view)
        {
            _logger = logger;
            _bus = bus;
            _writeModelRepository = repository;
            _readModelView = view;
        }

        #region Actions

        [HttpGet]
        public IEnumerable<EquipoDto> GetAll() =>
            _readModelView.FindAll();

        [HttpGet("{id}", Name = "GetEquipo")]
        public IActionResult GetById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            var item = _readModelView.Find(id);
            if (item == null)
                return NotFound(id);

            return new ObjectResult(item);
        }

        [HttpGet("{id}/{version}")]
        public IActionResult GetByIdVersion(Guid id, int version)
        {
            if (id == Guid.Empty || version < 0)
                return BadRequest();

            try
            {
                var equipo = _writeModelRepository.Find(id, version);
                return new ObjectResult(new EquipoDto(equipo));
            }
            catch (AggregateNotFoundException)
            {
                return NotFound(id);
            }
        }

        [HttpPost("{id}")]
        public IActionResult Crear(Guid id, [FromBody] CrearEquipo model)
        {
            if (model == null)
                return BadRequest();

            _bus.Send(new commands.CrearEquipo(id, model.Nombre));

            return CreatedAtRoute("GetEquipo", new { controller = "EquipoMontaje", id = id }, model);
        }

        [HttpPut("{id}/nombre")]
        public IActionResult ActualizarNombre(Guid id, [FromBody] ActualizarNombreEquipo model)
        {
            if (model == null)
                return BadRequest();

            _bus.Send(new commands.ActualizarNombreEquipo(id, model.NuevoNombre, model.OriginalVersion));

            return CreatedAtRoute("GetEquipo", new { controller = "EquipoMontaje", id = id }, model);
        }

        #endregion

        #region Logging

        public override BadRequestResult BadRequest()
        {
            _logger.LogWarning(EventIds.BadRequest, "Bad Request at {Action}", ControllerContext.ActionDescriptor.ActionName);
            return base.BadRequest();
        }

        public override NotFoundObjectResult NotFound(object id)
        {
            _logger.LogWarning(EventIds.NotFound, "{Id} Not Found at {Action}", id, ControllerContext.ActionDescriptor.ActionName);
            return base.NotFound(id);
        }

        static class EventIds
        {
            public const int BadRequest = 1;
            public const int NotFound = 2;
        }

        #endregion
    }
}

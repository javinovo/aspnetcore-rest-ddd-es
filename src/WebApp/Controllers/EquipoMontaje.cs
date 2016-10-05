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
    [Route("api/[controller]")]
    public class EquipoMontaje : Controller
    {
        EquiposRepository _repository;
        ICommandSender _bus;
        ILogger<EquipoMontaje> _logger;

        public EquipoMontaje(ILogger<EquipoMontaje> logger, ICommandSender bus, EquiposRepository repository)
        {
            _logger = logger;
            _bus = bus;
            _repository = repository;
        }

        #region Actions

        [HttpGet]
        public IEnumerable<EquipoDto> GetAll() =>
            EquiposView.DTOs;

        [HttpGet("{id}", Name = "GetEquipo")]
        public IActionResult GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                LogBadRequest(nameof(GetById));
                return BadRequest();
            }

            var item = EquiposView.Find(id);
            if (item == null)
            {
                LogNotFound(id, nameof(GetById));
                return NotFound();
            }

            return new ObjectResult(item);
        }

        [HttpGet("{id}/{version}")]
        public IActionResult GetByIdVersion(Guid id, int version)
        {
            if (id == Guid.Empty || version < 0)
            {
                LogBadRequest(nameof(GetByIdVersion));
                return BadRequest();
            }

            try
            {
                var equipo = _repository.Find(id, version);
                return new ObjectResult(new EquipoDto(equipo));
            }
            catch (AggregateNotFoundException)
            {
                LogNotFound(id, nameof(GetByIdVersion));
                return NotFound();
            }
        }

        [HttpPost("{id}")]
        public IActionResult Crear(Guid id, [FromBody] CrearEquipo model)
        {
            if (model == null)
            {
                LogBadRequest(nameof(Crear));
                return BadRequest();
            }

            _bus.Send(new commands.CrearEquipo(id, model.Nombre));

            return CreatedAtRoute("GetEquipo", new { controller = "EquipoMontaje", id = id }, model);
        }

        [HttpPut("{id}/nombre")]
        public IActionResult ActualizarNombre(Guid id, [FromBody] ActualizarNombreEquipo model)
        {
            if (model == null)
            {
                LogBadRequest(nameof(ActualizarNombre));
                return BadRequest();
            }

            _bus.Send(new commands.ActualizarNombreEquipo(id, model.NuevoNombre, model.OriginalVersion));

            return CreatedAtRoute("GetEquipo", new { controller = "EquipoMontaje", id = id }, model);
        }

        #endregion

        #region Logging

        void LogBadRequest(string action) =>
            _logger.LogWarning(EventIds.BadRequest, "Bad Request at {Action}", action);

        void LogNotFound(Guid id, string action) =>
            _logger.LogWarning(EventIds.NotFound, "{Id} Not Found at {Action}", id, action);

        static class EventIds
        {
            public const int BadRequest = 1;
            public const int NotFound = 2;
        }

        #endregion
    }
}

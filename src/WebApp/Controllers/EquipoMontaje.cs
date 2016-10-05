using BoundedContext.Montajes.Repositories;
using Domain.Exceptions;
using Infrastructure.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
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

        public EquipoMontaje(ICommandSender bus, EquiposRepository repository)
        {
            _bus = bus;
            _repository = repository;
        }

        [HttpGet]
        public IEnumerable<EquipoDto> GetAll() =>
            EquiposView.DTOs;

        [HttpGet("{id}", Name = "GetEquipo")]
        public IActionResult GetById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            var item = EquiposView.Find(id);
            if (item == null)
                return NotFound();

            return new ObjectResult(item);
        }

        [HttpGet("{id}/{version}")]
        public IActionResult GetById(Guid id, int version)
        {
            if (id == Guid.Empty || version < 0)
                return BadRequest();

            try
            {
                var equipo = _repository.Find(id, version);
                return new ObjectResult(new EquipoDto(equipo));
            }
            catch (AggregateNotFoundException)
            {
                return NotFound();
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
    }
}

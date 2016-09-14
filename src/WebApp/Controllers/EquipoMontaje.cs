using Infrastructure.Domain;
using Microsoft.AspNetCore.Mvc;
using ReadModel.Montajes.DTO;
using ReadModel.Montajes.Views;
using System;
using System.Collections.Generic;
using WebApp.Models;
using commands = BoundedContext.Montajes.Commands;

/*
curl -X POST -H "Content-Type: application/json" -H "Cache-Control: no-cache" -H "Postman-Token: 557f4036-3631-2e4a-8bf7-e9b98cc877cd" -d '{
	Nombre: "prueba"
}' "http://localhost:5000/api/EquipoMontaje/4cfcb48c-3ae3-41a4-834a-2adc12b19ed9/"

curl -X GET -H "Cache-Control: no-cache" -H "Postman-Token: 8350dc17-66ef-0338-fd98-f96e7764f633" "http://localhost:5000/api/EquipoMontaje/4cfcb48c-3ae3-41a4-834a-2adc12b19ed9"

curl -X PUT -H "Content-Type: application/json" -H "Cache-Control: no-cache" -H "Postman-Token: 1b866399-a099-612b-3b44-c173d9baff59" -d '{
	NuevoNombre: "actualizado!!",
	OriginalVersion: 0
}' "http://localhost:5000/api/EquipoMontaje/4cfcb48c-3ae3-41a4-834a-2adc12b19ed9/nombre"

curl -X GET -H "Cache-Control: no-cache" -H "Postman-Token: 8350dc17-66ef-0338-fd98-f96e7764f633" "http://localhost:5000/api/EquipoMontaje/4cfcb48c-3ae3-41a4-834a-2adc12b19ed9"
 */

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    public class EquipoMontaje : Controller
    {
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

        [HttpPost("{id}")]
        public IActionResult Crear(Guid id, [FromBody] CrearEquipo model)
        {
            if (model == null)
                return BadRequest();

            ServiceLocator.Bus.Send(new commands.CrearEquipo(id, model.Nombre));

            return CreatedAtRoute("GetEquipo", new { controller = "EquipoMontaje", id = id }, model);
        }

        [HttpPut("{id}/nombre")]
        public IActionResult ActualizarNombre(Guid id, [FromBody] ActualizarNombreEquipo model)
        {
            if (model == null)
                return BadRequest();

            ServiceLocator.Bus.Send(new commands.ActualizarNombreEquipo(id, model.NuevoNombre, model.OriginalVersion));

            return CreatedAtRoute("GetEquipo", new { controller = "EquipoMontaje", id = id }, model);
        }
    }
}

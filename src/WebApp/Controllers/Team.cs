using BoundedContext.Teams.Repositories;
using Domain.Exceptions;
using Halcyon.HAL;
using Halcyon.Web.HAL;
using Infrastructure.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReadModel.Teams.DTO;
using ReadModel.Teams.Views;
using System;
using System.Collections.Generic;
using WebApp.Models;
using commands = BoundedContext.Teams.Commands;

/*
curl -X GET -H "Cache-Control: no-cache" "http://localhost:5000/api/Team"

curl -X POST -H "Content-Type: application/json" -H "Cache-Control: no-cache" -d '{
	Name: "test"
}' "http://localhost:5000/api/Team/63931ea8-3f83-487c-8f21-01577a5157f9"

curl -X PUT -H "Content-Type: application/json" -H "Cache-Control: no-cache" -d '{
	NewName: "updated!!",
	OriginalVersion: 0
}' "http://localhost:5000/api/Team/63931ea8-3f83-487c-8f21-01577a5157f9/name"

curl -X GET -H "Cache-Control: no-cache" "http://localhost:5000/api/Team/63931ea8-3f83-487c-8f21-01577a5157f9"

curl -X GET -H "Cache-Control: no-cache" "http://localhost:5000/api/Team/63931ea8-3f83-487c-8f21-01577a5157f9/0"

curl -X DELETE -H "Content-Type: application/json" -H "Cache-Control: no-cache" -d '{
    OriginalVersion: 1
}' "http://localhost:5000/api/Team/63931ea8-3f83-487c-8f21-01577a5157f9"

 */

namespace WebApp.Controllers
{
    /// <summary>
    /// Facade exposed as a REST API which translates to queries and commands
    /// </summary>
    [Route("api/[controller]")]
    public class Team : Controller
    {
        ICommandSender _bus;
        ILogger<Team> _logger;
        TeamsView _readModelView;
        TeamRepository _writeModelRepository;

        public Team(ILogger<Team> logger, ICommandSender bus, TeamRepository repository, TeamsView view)
        {
            _logger = logger;
            _bus = bus;
            _writeModelRepository = repository;
            _readModelView = view;
        }

        #region Actions

        /// <summary>
        /// Get all teams.
        /// </summary>
        /// <returns>All the teams</returns>
        [HttpGet]
        public IEnumerable<TeamDto> GetAll() =>
            _readModelView.FindAll();

        /// <summary>
        /// Obtains a specific team from the read model. It should be the up to date version but it might not be.
        /// </summary>
        /// <returns>The team as currently stored in the read model or 404</returns>
        [HttpGet("{id:Guid}", Name = nameof(GetById))]
        public IActionResult GetById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            var item = _readModelView.Find(id);
            if (item == null)
                return NotFound(id);

            return this.HAL(item, new Link[]
            {
                new Link(Link.RelForSelf, "/api/Team/{Id}", replaceParameters: true),
                new Link("version", $"{Request.Path.Value}/{{version}}", "Specific version of the team's history", replaceParameters: false),
                new Link("updateName", "/api/Team/{Id}/name", method: "PUT"),
                new Link("dissolve", "/api/Team/{Id}", method: "DELETE")
            });
        }

        /// <summary>
        /// Obtains a team in a specific version by replaying all its events up to that version.
        /// </summary>
        /// <returns>The specified version of the team or 404</returns>
        [HttpGet("{id:Guid}/{version:int}")]
        public IActionResult GetByIdVersion(Guid id, int version)
        {
            if (id == Guid.Empty || version < 0)
                return BadRequest();

            try
            {
                var team = _writeModelRepository.Find(id, version);

                return this.HAL(new TeamDto(team), new Link[]
                {
                    new Link(Link.RelForSelf, Request.Path.Value),
                    new Link("last", "/api/Team/{Id}", title: "Up to date version", replaceParameters: true)
                });

            }
            catch (AggregateNotFoundException)
            {
                return NotFound(id);
            }
        }

        /// <summary>
        /// Creates a new team.
        /// </summary>
        [HttpPost("{id:Guid}")]
        public IActionResult Create(Guid id, [FromBody] CreateTeam model)
        {
            if (model == null)
                return BadRequest();

            _bus.Send(new commands.CreateTeam(id, model.Name));

            return CreatedAtRoute(nameof(GetById), new { controller = nameof(Team), id = id }, model);
        }

        /// <summary>
        /// Updates the name of a team
        /// </summary>
        [HttpPut("{id:Guid}/name")]
        public IActionResult UpdateName(Guid id, [FromBody] UpdateTeamName model)
        {
            if (model == null)
                return BadRequest();

            _bus.Send(new commands.UpdateTeamName(id, model.NewName, model.OriginalVersion));

            return CreatedAtRoute(nameof(GetById), new { controller = nameof(Team), id = id }, model);
        }

        [HttpDelete("{id:Guid}")]
        public IActionResult Delete(Guid id, [FromBody] DissolveTeam model)
        {
            _bus.Send(new commands.DissolveTeam(id, model.OriginalVersion));

            return Ok();
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

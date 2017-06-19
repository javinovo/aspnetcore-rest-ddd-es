using BoundedContext.Teams.Repositories;
using Domain.Exceptions;
using Infrastructure.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReadModel.Teams.DTO;
using ReadModel.Teams.Views;
using System;
using System.Collections.Generic;
using WebApp.Models;
using commands = BoundedContext.Teams.Commands;
using Halcyon.HAL;
using Halcyon.Web.HAL;

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
        [HttpGet("{id}", Name = "GetTeam")]
        public IActionResult GetById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            var item = _readModelView.Find(id);
            if (item == null)
                return NotFound(id);

            return this.HAL(item, new Link[] { new Link(Link.RelForSelf, "/api/{id}") });
            //return new ObjectResult(item);
        }

        /// <summary>
        /// Obtains a team in a specific version by replaying all its events up to that version.
        /// </summary>
        /// <returns>The specified version of the team or 404</returns>
        [HttpGet("{id}/{version}")]
        public IActionResult GetByIdVersion(Guid id, int version)
        {
            if (id == Guid.Empty || version < 0)
                return BadRequest();

            try
            {
                var team = _writeModelRepository.Find(id, version);
                return new ObjectResult(new TeamDto(team));
            }
            catch (AggregateNotFoundException)
            {
                return NotFound(id);
            }
        }

        /// <summary>
        /// Creates a new team.
        /// </summary>
        [HttpPost("{id}")]
        public IActionResult Create(Guid id, [FromBody] CreateTeam model)
        {
            if (model == null)
                return BadRequest();

            _bus.Send(new commands.CreateTeam(id, model.Name));

            return CreatedAtRoute("GetTeam", new { controller = "Team", id = id }, model);
        }

        /// <summary>
        /// Updates the name of a team
        /// </summary>
        [HttpPut("{id}/name")]
        public IActionResult UpdateName(Guid id, [FromBody] UpdateTeamName model)
        {
            if (model == null)
                return BadRequest();

            _bus.Send(new commands.UpdateTeamName(id, model.NewName, model.OriginalVersion));

            return CreatedAtRoute("GetTeam", new { controller = "Team", id = id }, model);
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

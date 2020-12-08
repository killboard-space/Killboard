using System;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Killboard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class RouteController : ControllerBase
    {
        private readonly ISystemRepository _systemRepository;

        public RouteController(ISystemRepository systemRepository)
        {
            _systemRepository = systemRepository;
        }

        [HttpGet("{fromSystemId}/{toSystemId}")]
        [EnableCors("AllowOrigin")]
        public IActionResult GetRoute(int fromSystemId, int toSystemId)
        {
            try
            {
                return Ok(_systemRepository.GetRoute(fromSystemId, toSystemId));
            }
            catch (ApplicationException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("range/{fromSystemId}/{jumps}")]
        [EnableCors("AllowOrigin")]
        public IActionResult GetSystemsInRange(int fromSystemId, int jumps)
        {
            try
            {
                return Ok(_systemRepository.GetSystemsInRange(fromSystemId, jumps));
            }
            catch (ApplicationException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}

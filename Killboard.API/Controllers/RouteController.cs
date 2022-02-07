using System;
using System.Threading.Tasks;
using Killboard.Domain.DTO.Universe;
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

        [HttpPost("jumprange")]
        [EnableCors("AllowOrigin")]
        public async Task<IActionResult> GetSystemsInJumpRange([FromBody] PostJumpRangeParameters parameters)
        {
            try
            {
                var result = await _systemRepository.GetSystemsInJumpRange(parameters.FromSystemId, parameters.ShipId,
                    parameters.JumpDriveCalibrationLevel);
                return Ok(result);
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

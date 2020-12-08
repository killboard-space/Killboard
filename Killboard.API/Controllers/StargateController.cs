using System;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Killboard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StargateController : ControllerBase
    {
        private readonly IStargateRepository _stargateRepository;

        public StargateController(IStargateRepository stargateRepository)
        {
            _stargateRepository = stargateRepository;
        }

        // GET: api/Stargate
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(_stargateRepository.GetAll());
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

        // GET: api/Stargate/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                return Ok(_stargateRepository.GetStargate(id));
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

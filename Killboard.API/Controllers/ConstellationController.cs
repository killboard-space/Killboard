using System;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Killboard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConstellationController : ControllerBase
    {
        private readonly IConstellationRepository _constellationRepository;

        public ConstellationController(IConstellationRepository constellationRepository)
        {
            _constellationRepository = constellationRepository;
        }

        // GET: api/Constellation
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(_constellationRepository.GetAll());
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

        // GET: api/Constellation/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                return Ok(_constellationRepository.GetConstellation(id));
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

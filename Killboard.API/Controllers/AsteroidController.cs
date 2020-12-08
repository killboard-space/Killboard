using System;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Killboard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsteroidController : ControllerBase
    {
        private readonly IAsteroidRepository _asteroidRepository;

        public AsteroidController(IAsteroidRepository asteroidRepository)
        {
            _asteroidRepository = asteroidRepository;
        }

        // GET: api/Asteroid
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(_asteroidRepository.GetAll());
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

        // GET: api/Asteroid/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                return Ok(_asteroidRepository.GetAsteroid(id));
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

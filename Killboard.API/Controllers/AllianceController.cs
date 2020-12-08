using System;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Killboard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AllianceController : ControllerBase
    {
        private readonly IAllianceRepository _allianceRepository;

        public AllianceController(IAllianceRepository allianceRepository)
        {
            _allianceRepository = allianceRepository;
        }

        // GET: api/Alliance
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(_allianceRepository.GetAll());
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

        // GET: api/Alliance/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                return Ok(_allianceRepository.GetAlliance(id));
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

        // GET: api/Alliance/5/detail
        [HttpGet("{id}/detail")]
        public IActionResult GetDetail(int id)
        {
            try
            {
                return Ok(_allianceRepository.GetAllianceDetail(id));
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

        // POST: api/Alliance
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        // PUT: api/Alliance/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        // DELETE: api/ApiWithActions/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}

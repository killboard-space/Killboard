using System;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Killboard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CorporationController : ControllerBase
    {
        private readonly ICorporationRepository _corporationRepository;

        public CorporationController(ICorporationRepository corporationRepository)
        {
            _corporationRepository = corporationRepository;
        }

        // GET: api/Corporation
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(_corporationRepository.GetAll());
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

        // GET: api/Corporation/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                return Ok(_corporationRepository.GetCorporation(id));
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

        // GET: api/Corporation/5/detail
        [HttpGet("{id}/detail")]
        public IActionResult GetDetail(int id)
        {
            try
            {
                return Ok(_corporationRepository.GetCorporationDetail(id));
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

        // POST: api/Corporation
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        // PUT: api/Corporation/5
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

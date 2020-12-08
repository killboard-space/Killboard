using System;
using System.Collections.Generic;
using System.Linq;
using Killboard.Domain.DTO.Universe.System;
using Killboard.Domain.Interfaces;
using Killboard.Domain.Params;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Killboard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly ISystemRepository _systemRepository;

        public SystemController(ISystemRepository systemRepository)
        {
            _systemRepository = systemRepository;
        }

        // GET: api/System
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult Get([FromQuery] GetAllSystemParameters parameters)
        {
            try
            {
                (IEnumerable<GetSystem> systems, int pages) = _systemRepository.GetAll(parameters);
                Response.Headers.Add("X-Pages", $"{pages}");
                Response.Headers.Add("X-Count", $"{systems.Count()}");
                Response.Headers.Add("Access-Control-Expose-Headers", "X-Pages,X-Count");
                return Ok(systems);
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

        // GET: api/System/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                return Ok(_systemRepository.GetSystem(id));
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

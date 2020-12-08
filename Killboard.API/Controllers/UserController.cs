using System;
using System.Threading.Tasks;
using Killboard.Domain.DTO.User;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Killboard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _usersRepository;
        public UserController(IUserRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        // GET: api/User

        /// <summary>
        /// Attempts to retrieve a list of registered users represented as public objects.
        /// </summary>
        /// <returns>Returns a list of all registered users.</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                return Ok(_usersRepository.GetUsers());
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

        // GET: api/User/5

        /// <summary>
        /// Attempts to retrieve a specific registered user.
        /// </summary>
        /// <param name="id">Character ID</param>
        /// <returns>The registered user represented as a public object.</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                return Ok(_usersRepository.GetUser(id));
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

        // GET: api/User/5/Email

        /// <summary>
        /// Attempts to retrieve a specific registered user's email.
        /// </summary>
        /// <param name="id">Character ID</param>
        /// <returns>The registered user's email.</returns>
        [HttpGet("{id}/Email")]
        public IActionResult GetEmail(int id)
        {
            try
            {
                return Ok(_usersRepository.GetEmail(id));
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

        // GET: api/User/5/Characters

        /// <summary>
        /// Attempts to retrieve a specific registered user's authorized Eve Online characters.
        /// </summary>
        /// <param name="id">Character ID</param>
        /// <returns>A list containing all authorized character information.</returns>
        [HttpGet("{id}/Characters")]
        public IActionResult GetCharacters(int id)
        {
            try
            {
                return Ok(_usersRepository.GetCharacters(id));
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

        // GET: api/User/5/Characters/short

        /// <summary>
        /// Attempts to retrieve a specific registered user's authorized Eve Online characters.
        /// </summary>
        /// <param name="id">Character ID</param>
        /// <returns>A list containing all authorized character information.</returns>
        [HttpGet("{id}/Characters/short")]
        public IActionResult GetCharactersShort(int id)
        {
            try
            {
                return Ok(JsonConvert.SerializeObject(_usersRepository.GetCharactersList(id)));
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

        // POST: api/User
        [HttpPost]
        public IActionResult Post([FromBody] PostUser user)
        {
            try
            {
                return Ok(_usersRepository.AddUser(user));
            }
            catch(ApplicationException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // POST: api/User/Authenticate
        [HttpPost("Authenticate")]
        public IActionResult Post([FromBody] PostAuthenticateUser user)
        {
            try
            {
                return Ok(_usersRepository.Authenticate(user));
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

        // POST: api/User/Forget
        [HttpPost("Forget")]
        public async Task<IActionResult> Post([FromBody] ForgetUser user)
        {
            try
            {
                await _usersRepository.AddResetRequest(user);

                return Ok();
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

        // POST: api/User/KeyValidation
        [HttpPost("KeyValidation")]
        public IActionResult Post([FromBody] KeyValidation validation)
        {
            try
            {
                return Ok(_usersRepository.ValidateKey(validation));
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

        // POST: api/User/Forget
        [HttpPost("Change")]
        public IActionResult Post([FromBody] ChangeRequest request)
        {
            try
            {
                return Ok(_usersRepository.ChangePassword(request));
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

        // PUT: api/User/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IAuthService _authService;
        private readonly IMongoLogService _logService;

        public UsersController(UserContext context, IAuthService authService, IMongoLogService logService)
        {
            _context = context;
            _authService = authService;
            _logService = logService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        // POST: api/Users
        // Compatible with Postman collection and original Hubbec spec
        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] RegisterDto registerDto)
        {
            var (success, errorMessage, response) = await _authService.RegisterAsync(registerDto);
            if (!success)
            {
                return BadRequest(new { alert = errorMessage, status = 400 });
            }

            var createdUser = await _context.Users.FindAsync(response!.userId);
            return CreatedAtAction("GetUser", new { id = response.userId }, createdUser);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutUser(long id, [FromBody] User user)
        {
            if (id != user.Id)
            {
                return BadRequest(new { alert = "ID no coincide con el registro a editar", status = 400 });
            }

            var existing = await _context.Users.FindAsync(id);
            if (existing == null)
            {
                return NotFound(new { alert = "Usuario no encontrado", status = 404 });
            }

            existing.firstName = user.firstName;
            existing.lastName = user.lastName;
            existing.phone = user.phone;

            await _context.SaveChangesAsync();
            await _logService.LogEventAsync("USER_UPDATED", $"User profile {id} updated", existing.email);

            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> DeleteUser(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await _logService.LogEventAsync("USER_DELETED", $"User {id} deleted", user.email);

            return user;
        }
    }
}

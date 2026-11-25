using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactsController(IContactService contactService)
        {
            _contactService = contactService;
        }

        // GET: api/Contacts
        // If authorized, returns contacts for current user. If id is passed, returns contacts for that user id.
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetContacts()
        {
            long userId = GetCurrentUserId();
            var contacts = await _contactService.GetUserContactsAsync(userId);
            return Ok(contacts);
        }

        // GET: api/Contacts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContactsById(long id)
        {
            // If ID matches a user ID, get their contacts; if matches contact ID, return contact
            var userContacts = await _contactService.GetUserContactsAsync(id);
            if (userContacts.Count > 0)
            {
                return Ok(userContacts);
            }

            var contact = await _contactService.GetContactByIdAsync(id);
            if (contact != null)
            {
                return Ok(contact);
            }

            return Ok(new List<ContactDto>());
        }

        // GET: api/Contacts/{id}/details
        // Requirement 8: Shows basic info + mutual contacts + non-mutual contacts
        [HttpGet("{id}/details")]
        [Authorize]
        public async Task<IActionResult> GetContactDetails(long id)
        {
            long currentUserId = GetCurrentUserId();
            var details = await _contactService.GetContactDetailsAsync(currentUserId, id);

            if (details == null)
            {
                return NotFound(new { alert = "Contacto no encontrado", status = 404 });
            }

            return Ok(details);
        }

        // POST: api/Contacts
        [HttpPost]
        public async Task<IActionResult> PostContacts([FromBody] CreateContactDto createDto)
        {
            if (createDto.idUser <= 0)
            {
                createDto.idUser = GetCurrentUserId();
            }

            if (createDto.idUser <= 0)
            {
                return BadRequest(new { alert = "El ID del usuario es obligatorio", status = 400 });
            }

            var (success, message, contact) = await _contactService.AddContactAsync(createDto);
            if (!success)
            {
                return BadRequest(new { alert = message, status = 400 });
            }

            return Ok(new
            {
                alert = message,
                status = 200,
                contact = contact
            });
        }

        // PUT: api/Contacts/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutContacts(long id, [FromBody] UpdateContactDto updateDto)
        {
            var (success, message) = await _contactService.UpdateContactAsync(id, updateDto);
            if (!success)
            {
                return BadRequest(new { alert = message, status = 400 });
            }

            return Ok(new { alert = message, status = 200 });
        }

        // DELETE: api/Contacts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContacts(long id)
        {
            long currentUserId = GetCurrentUserId();
            var (success, message) = await _contactService.DeleteContactAsync(id, currentUserId);

            if (!success)
            {
                return BadRequest(new { alert = message, status = 400 });
            }

            return Ok(new
            {
                alert = message,
                status = 200
            });
        }

        // GET: api/Contacts/search-directory
        [HttpGet("search-directory")]
        [Authorize]
        public async Task<IActionResult> SearchDirectory([FromQuery] string? q)
        {
            long currentUserId = GetCurrentUserId();
            var users = await _contactService.SearchDirectoryUsersAsync(currentUserId, q);
            return Ok(users);
        }

        private long GetCurrentUserId()
        {
            string? userIdStr = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdStr, out long userId))
            {
                return userId;
            }
            return 0;
        }
    }
}

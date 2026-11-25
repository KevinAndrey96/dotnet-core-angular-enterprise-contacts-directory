using System;
using System.Threading.Tasks;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly UserContext _context;

        public HealthController(UserContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            bool databaseHealthy = false;
            int userCount = 0;
            int contactCount = 0;

            try
            {
                databaseHealthy = await _context.Database.CanConnectAsync();
                if (databaseHealthy)
                {
                    userCount = await _context.Users.CountAsync();
                    contactCount = await _context.Contacts.CountAsync();
                }
            }
            catch
            {
                databaseHealthy = false;
            }

            return Ok(new
            {
                status = databaseHealthy ? "HEALTHY" : "DEGRADED",
                timestamp = DateTime.UtcNow,
                service = "Contacts Directory Enterprise API",
                version = "2.0.0",
                database = new
                {
                    connected = databaseHealthy,
                    users = userCount,
                    contacts = contactCount
                }
            });
        }
    }
}

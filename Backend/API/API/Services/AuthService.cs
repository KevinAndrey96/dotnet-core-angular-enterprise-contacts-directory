using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.DTOs;
using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMongoLogService _logService;

        public AuthService(UserContext context, IConfiguration configuration, IMongoLogService logService)
        {
            _context = context;
            _configuration = configuration;
            _logService = logService;
        }

        public string GenerateJwtToken(User user)
        {
            string secretKey = _configuration.GetValue<string>("SecretKey") ?? "DefaultSuperSecretKeyForContactsDirectory2026";
            string issuer = _configuration.GetValue<string>("Issuer") ?? "localhost";
            string audience = _configuration.GetValue<string>("Audience") ?? "localhost";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim("firstName", user.firstName),
                new Claim("lastName", user.lastName),
                new Claim(JwtRegisteredClaimNames.Email, user.email),
                new Claim("documentType", user.documentType),
                new Claim("documentNumber", user.documentNumber)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<AuthResponseDto?> AuthenticateAsync(LoginDto loginDto)
        {
            string email = loginDto.Username.Trim().ToLowerInvariant();
            string password = loginDto.Pass;

            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.email.ToLower() == email && u.password == password);

            if (user == null)
            {
                await _logService.LogEventAsync("LOGIN_FAILED", $"Failed login attempt for {email}", email);
                return null;
            }

            string token = GenerateJwtToken(user);
            await _logService.LogEventAsync("LOGIN_SUCCESS", $"User {email} logged in successfully", email);

            return new AuthResponseDto
            {
                token = token,
                userId = user.Id,
                firstName = user.firstName,
                lastName = user.lastName,
                email = user.email
            };
        }

        public async Task<(bool Success, string? ErrorMessage, AuthResponseDto? Response)> RegisterAsync(RegisterDto registerDto)
        {
            string email = registerDto.email.Trim().ToLowerInvariant();

            // Requirement 2: No duplicate user with same email and documentNumber
            bool userExists = await _context.Users.AnyAsync(u => 
                u.email.ToLower() == email || 
                (u.documentNumber == registerDto.documentNumber && u.documentType == registerDto.documentType));

            if (userExists)
            {
                return (false, "Ya existe un usuario registrado con este correo o documento de identidad.", null);
            }

            // Requirement 3: Check if already registered as a contact by someone else
            var existingContactRecords = await _context.Contacts
                .Where(c => c.email.ToLower() == email || c.documentNumber == registerDto.documentNumber)
                .ToListAsync();

            string? registeredByMessage = null;
            if (existingContactRecords.Any())
            {
                var registeredByUserIds = existingContactRecords.Select(c => c.IdUser).Distinct().ToList();
                var registeredByUsers = await _context.Users
                    .Where(u => registeredByUserIds.Contains(u.Id))
                    .Select(u => $"{u.firstName} {u.lastName}")
                    .ToListAsync();

                if (registeredByUsers.Any())
                {
                    registeredByMessage = $"Aviso: Fuiste registrado previamente como contacto por: {string.Join(", ", registeredByUsers)}. Tu cuenta ha sido activada y tus datos actualizados.";
                }
            }

            // Create User
            var newUser = new User
            {
                firstName = registerDto.firstName.Trim(),
                lastName = registerDto.lastName.Trim(),
                documentType = registerDto.documentType.Trim(),
                documentNumber = registerDto.documentNumber.Trim(),
                phone = registerDto.phone.Trim(),
                email = email,
                password = registerDto.password,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Link existing contact records to this new registered user ID
            if (existingContactRecords.Any())
            {
                foreach (var contactRecord in existingContactRecords)
                {
                    contactRecord.IdContact = newUser.Id;
                    contactRecord.firstName = newUser.firstName;
                    contactRecord.lastName = newUser.lastName;
                    contactRecord.phone = newUser.phone;
                }
                await _context.SaveChangesAsync();
            }

            await _logService.LogEventAsync("USER_REGISTERED", $"New user registered: {newUser.email}", newUser.email);

            string token = GenerateJwtToken(newUser);

            var response = new AuthResponseDto
            {
                token = token,
                userId = newUser.Id,
                firstName = newUser.firstName,
                lastName = newUser.lastName,
                email = newUser.email,
                registeredByMessage = registeredByMessage
            };

            return (true, null, response);
        }
    }
}

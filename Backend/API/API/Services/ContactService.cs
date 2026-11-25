using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class ContactService : IContactService
    {
        private readonly UserContext _context;
        private readonly IMongoLogService _logService;

        public ContactService(UserContext context, IMongoLogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<List<ContactDto>> GetUserContactsAsync(long userId)
        {
            var contacts = await _context.Contacts
                .Where(c => c.IdUser == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return contacts.Select(c => MapToDto(c)).ToList();
        }

        public async Task<ContactDto?> GetContactByIdAsync(long contactId)
        {
            var contact = await _context.Contacts.FindAsync(contactId);
            return contact == null ? null : MapToDto(contact);
        }

        public async Task<ContactDetailsDto?> GetContactDetailsAsync(long currentUserId, long contactId)
        {
            var contact = await _context.Contacts.FindAsync(contactId);
            if (contact == null) return null;

            var detailsDto = new ContactDetailsDto
            {
                contact = MapToDto(contact),
                isRegisteredUser = contact.IdContact.HasValue && contact.IdContact.Value > 0
            };

            // If contact is a registered user, calculate mutual and non-mutual contacts (Requirement 8)
            if (detailsDto.isRegisteredUser)
            {
                long targetUserId = contact.IdContact!.Value;

                // User A's (current user) contacts
                var myContacts = await _context.Contacts
                    .Where(c => c.IdUser == currentUserId)
                    .ToListAsync();

                // User B's (target contact) contacts
                var targetUserContacts = await _context.Contacts
                    .Where(c => c.IdUser == targetUserId && c.IdContact != currentUserId)
                    .ToListAsync();

                var myContactEmails = new HashSet<string>(myContacts
                    .Where(c => !string.IsNullOrEmpty(c.email))
                    .Select(c => c.email.ToLower()));

                var myContactDocNumbers = new HashSet<string>(myContacts
                    .Where(c => !string.IsNullOrEmpty(c.documentNumber))
                    .Select(c => c.documentNumber));

                var mutualList = new List<ContactDto>();
                var otherList = new List<ContactDto>();

                foreach (var tc in targetUserContacts)
                {
                    bool isMutual = (!string.IsNullOrEmpty(tc.email) && myContactEmails.Contains(tc.email.ToLower())) ||
                                    (!string.IsNullOrEmpty(tc.documentNumber) && myContactDocNumbers.Contains(tc.documentNumber));

                    if (isMutual)
                    {
                        mutualList.Add(MapToDto(tc));
                    }
                    else
                    {
                        otherList.Add(MapToDto(tc));
                    }
                }

                detailsDto.mutualContacts = mutualList;
                detailsDto.otherContacts = otherList;
            }

            return detailsDto;
        }

        public async Task<(bool Success, string Message, ContactDto? Contact)> AddContactAsync(CreateContactDto createDto)
        {
            string email = (createDto.email ?? "").Trim().ToLowerInvariant();
            string docNumber = (createDto.documentNumber ?? "").Trim();

            // Check if user is trying to add themselves
            if (createDto.idContact.HasValue && createDto.idContact.Value == createDto.idUser)
            {
                return (false, "No puedes agregarte a ti mismo como contacto.", null);
            }

            // Check if contact already exists in user's address book
            bool alreadyExists = await _context.Contacts.AnyAsync(c =>
                c.IdUser == createDto.idUser &&
                ((createDto.idContact.HasValue && c.IdContact == createDto.idContact.Value) ||
                 (!string.IsNullOrEmpty(email) && c.email.ToLower() == email) ||
                 (!string.IsNullOrEmpty(docNumber) && c.documentNumber == docNumber)));

            if (alreadyExists)
            {
                return (false, "Este contacto ya se encuentra en tu lista de contactos.", null);
            }

            // Check if contact exists as a registered user in directory
            User? linkedUser = null;
            if (createDto.idContact.HasValue && createDto.idContact.Value > 0)
            {
                linkedUser = await _context.Users.FindAsync(createDto.idContact.Value);
            }
            else if (!string.IsNullOrEmpty(email))
            {
                linkedUser = await _context.Users.FirstOrDefaultAsync(u => u.email.ToLower() == email);
            }
            else if (!string.IsNullOrEmpty(docNumber))
            {
                linkedUser = await _context.Users.FirstOrDefaultAsync(u => u.documentNumber == docNumber);
            }

            var newContact = new Contacts
            {
                IdUser = createDto.idUser,
                IdContact = linkedUser?.Id ?? createDto.idContact,
                firstName = linkedUser?.firstName ?? createDto.firstName ?? "",
                lastName = linkedUser?.lastName ?? createDto.lastName ?? "",
                documentType = linkedUser?.documentType ?? createDto.documentType ?? "CC",
                documentNumber = linkedUser?.documentNumber ?? createDto.documentNumber ?? "",
                phone = linkedUser?.phone ?? createDto.phone ?? "",
                email = linkedUser?.email ?? createDto.email ?? "",
                relationship = string.IsNullOrEmpty(createDto.relationship) ? "Amigo" : createDto.relationship,
                CreatedAt = DateTime.UtcNow
            };

            _context.Contacts.Add(newContact);
            await _context.SaveChangesAsync();

            await _logService.LogEventAsync("CONTACT_ADDED", 
                $"User {createDto.idUser} added contact {newContact.firstName} {newContact.lastName} ({newContact.relationship})",
                newContact.email);

            return (true, "Contacto agregado con éxito.", MapToDto(newContact));
        }

        public async Task<(bool Success, string Message)> UpdateContactAsync(long contactId, UpdateContactDto updateDto)
        {
            var contact = await _context.Contacts.FindAsync(contactId);
            if (contact == null)
            {
                return (false, "Contacto no encontrado.");
            }

            contact.relationship = updateDto.relationship;
            if (!string.IsNullOrEmpty(updateDto.phone)) contact.phone = updateDto.phone;
            if (!string.IsNullOrEmpty(updateDto.email)) contact.email = updateDto.email;

            await _context.SaveChangesAsync();

            await _logService.LogEventAsync("CONTACT_UPDATED", 
                $"Contact {contactId} updated to relationship {contact.relationship}", contact.email);

            return (true, "Contacto actualizado con éxito.");
        }

        public async Task<(bool Success, string Message)> DeleteContactAsync(long contactId, long currentUserId)
        {
            var contact = await _context.Contacts.FindAsync(contactId);
            if (contact == null)
            {
                return (false, "Contacto no encontrado.");
            }

            if (currentUserId > 0 && contact.IdUser != currentUserId)
            {
                return (false, "No tienes permisos para eliminar este contacto.");
            }

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            await _logService.LogEventAsync("CONTACT_DELETED", 
                $"Contact {contact.firstName} {contact.lastName} deleted by user {currentUserId}", contact.email);

            return (true, "Contacto eliminado con éxito.");
        }

        public async Task<List<ContactDto>> SearchDirectoryUsersAsync(long currentUserId, string? query)
        {
            query = (query ?? "").Trim().ToLower();

            // Get users in directory except current user
            var usersQuery = _context.Users.Where(u => u.Id != currentUserId);

            if (!string.IsNullOrEmpty(query))
            {
                usersQuery = usersQuery.Where(u =>
                    u.firstName.ToLower().Contains(query) ||
                    u.lastName.ToLower().Contains(query) ||
                    u.email.ToLower().Contains(query) ||
                    u.documentNumber.Contains(query));
            }

            var users = await usersQuery.Take(20).ToListAsync();

            // Check which ones are already contacts
            var myContactUserIds = await _context.Contacts
                .Where(c => c.IdUser == currentUserId && c.IdContact.HasValue)
                .Select(c => c.IdContact!.Value)
                .ToListAsync();

            return users.Select(u => new ContactDto
            {
                id = 0,
                idUser = currentUserId,
                idContact = u.Id,
                firstName = u.firstName,
                lastName = u.lastName,
                documentType = u.documentType,
                documentNumber = u.documentNumber,
                phone = u.phone,
                email = u.email,
                relationship = myContactUserIds.Contains(u.Id) ? "Ya asociado" : "Disponible",
                isRegisteredUser = true
            }).ToList();
        }

        private static ContactDto MapToDto(Contacts c)
        {
            return new ContactDto
            {
                id = c.Id,
                idUser = c.IdUser,
                idContact = c.IdContact,
                firstName = c.firstName,
                lastName = c.lastName,
                documentType = c.documentType,
                documentNumber = c.documentNumber,
                phone = c.phone,
                email = c.email,
                relationship = c.relationship,
                isRegisteredUser = c.IdContact.HasValue && c.IdContact.Value > 0
            };
        }
    }
}

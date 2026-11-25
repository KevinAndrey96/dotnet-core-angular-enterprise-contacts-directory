using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;

namespace API.Services
{
    public interface IContactService
    {
        Task<List<ContactDto>> GetUserContactsAsync(long userId);
        Task<ContactDto?> GetContactByIdAsync(long contactId);
        Task<ContactDetailsDto?> GetContactDetailsAsync(long currentUserId, long contactId);
        Task<(bool Success, string Message, ContactDto? Contact)> AddContactAsync(CreateContactDto createDto);
        Task<(bool Success, string Message)> UpdateContactAsync(long contactId, UpdateContactDto updateDto);
        Task<(bool Success, string Message)> DeleteContactAsync(long contactId, long currentUserId);
        Task<List<ContactDto>> SearchDirectoryUsersAsync(long currentUserId, string? query);
    }
}

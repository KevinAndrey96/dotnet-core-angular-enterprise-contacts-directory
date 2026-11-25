using System.Collections.Generic;

namespace API.DTOs
{
    public class ContactDetailsDto
    {
        public ContactDto contact { get; set; } = new ContactDto();
        public bool isRegisteredUser { get; set; }
        public List<ContactDto> mutualContacts { get; set; } = new List<ContactDto>();
        public List<ContactDto> otherContacts { get; set; } = new List<ContactDto>();
    }
}

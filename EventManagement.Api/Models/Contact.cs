using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Models
{
    public class Contact
    {
        public int ContactId { get; set; }
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
        public int? AddressId { get; set; }
    }
}
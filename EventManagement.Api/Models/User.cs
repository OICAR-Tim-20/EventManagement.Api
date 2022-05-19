using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Models
{
    public class User
    {
        public int UserId { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public byte[] PasswordHash { get; set; }
        [Required]
        public byte[] PasswordSalt { get; set; }
        [Required]
        public string Email { get; set; }
        public Address Address { get; set; }
        public string Picture { get; set; }
        public string ContactName { get; set; }
        public string PhoneNumber { get; set; }
        public ICollection<Event> Events { get; set; }
        public int? AddressId { get; set; }
        public int UserType { get; set; }
    }
}
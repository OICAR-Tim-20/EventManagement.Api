using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Models
{
    public class Location
    {
        public int LocationId { get; set; }
        [Required]
        public Address Address { get; set; }
        [Required]
        public int AddressId { get; set; }
        public string Venue { get; set; }
    }
}
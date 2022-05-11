using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Models
{
    public class Address
    {

        public int AddressId { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string ZipCode { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
    }
}
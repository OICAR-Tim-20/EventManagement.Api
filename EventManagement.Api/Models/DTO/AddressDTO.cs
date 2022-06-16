namespace EventManagement.Api.Models.DTO
{
    public class AddressDTO
    {
        public int AddressId { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
    }
}

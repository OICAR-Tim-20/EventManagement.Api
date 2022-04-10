namespace EventManagement.Api.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public Address Address { get; set; }
        public string? Picture { get; set; }
        public string? ContactName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
namespace EventManagement.Api.Models.DTO
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Address? Address { get; set; } = null;
        public string? Picture { get; set; } = null;
        public string ContactName { get; set; }
        public string PhoneNumber { get; set; }
    }
}

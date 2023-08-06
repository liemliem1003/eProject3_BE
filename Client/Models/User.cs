namespace Client.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Name { get; set; }
        public DateTime? Dob { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public int? Role { get; set; }
        public bool? Status { get; set; }
    }
}
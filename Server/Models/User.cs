using System;
using System.Collections.Generic;

namespace Server.Models
{
    public partial class User
    {
        public User()
        {
            Claims = new HashSet<Claim>();
            PolicyOnUsers = new HashSet<PolicyOnUser>();
        }

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

        public virtual ICollection<Claim> Claims { get; set; }
        public virtual ICollection<PolicyOnUser> PolicyOnUsers { get; set; }
    }
}

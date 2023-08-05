using System;
using System.Collections.Generic;

namespace Server.Models
{
    public partial class Policy
    {
        public Policy()
        {
            Claims = new HashSet<Claim>();
            PolicyOnUsers = new HashSet<PolicyOnUser>();
        }

        public int PolicyId { get; set; }
        public string? PolicyName { get; set; }
        public string? Desciption { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? Duration { get; set; }
        public int? CompanyId { get; set; }
        public string? Banner { get; set; }
        public bool? Status { get; set; }

        public virtual Company? Company { get; set; }
        public virtual ICollection<Claim> Claims { get; set; }
        public virtual ICollection<PolicyOnUser> PolicyOnUsers { get; set; }
    }
}

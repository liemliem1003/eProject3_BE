using System;
using System.Collections.Generic;

namespace Server.Models
{
    public partial class Company
    {
        public Company()
        {
            Policies = new HashSet<Policy>();
        }

        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyPhone { get; set; }
        public string? Address { get; set; }
        public string? Logo { get; set; }
        public string? Url { get; set; }
        public bool? Status { get; set; }

        public virtual ICollection<Policy> Policies { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Server.Models
{
    public partial class Claim
    {
        public Claim()
        {
            ClaimImages = new HashSet<ClaimImage>();
        }

        public int ClaimId { get; set; }
        public string? Description { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? UserId { get; set; }
        public decimal? AppAmount { get; set; }
        public bool? Status { get; set; }
        public int? PolicyId { get; set; }

        public virtual Policy? Policy { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<ClaimImage> ClaimImages { get; set; }
    }
}

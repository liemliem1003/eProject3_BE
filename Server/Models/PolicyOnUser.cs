using System;
using System.Collections.Generic;

namespace Server.Models
{
    public partial class PolicyOnUser
    {
        public int Id { get; set; }
        public int? PolicyId { get; set; }
        public int? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? AvaibleAmount { get; set; }

        public virtual Policy? Policy { get; set; }
        public virtual User? User { get; set; }
    }
}

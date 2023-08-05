using System;
using System.Collections.Generic;

namespace Server.Models
{
    public partial class ClaimImage
    {
        public int ImageId { get; set; }
        public int? ClaimId { get; set; }
        public string? Url { get; set; }

        public virtual Claim? Claim { get; set; }
    }
}

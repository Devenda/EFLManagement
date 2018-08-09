using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EFLManagementAPI.Entities
{
    public class Presence
    {
        [Key]
        public int PresenceId { get; set; }

        [Required]
        public User User { get; set; }

        [Required]
        public DateTime TimestampScan { get; set; }

    }
}

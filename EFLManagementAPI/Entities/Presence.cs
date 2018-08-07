using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFLManagementAPI.Entities
{
    public class Presence
    {
        public int PresenceId { get; set; }
        public virtual User User { get; set; }
        public DateTime TimestampScan { get; set; }

    }
}

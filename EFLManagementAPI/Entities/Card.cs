using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFLManagementAPI.Entities
{
    public class Card
    {
        public int CardId { get; set; }
        public string CardCode { get; set; }
        public virtual User User { get; set; }
        public DateTime TimestampRegistration { get; set; }

    }
}

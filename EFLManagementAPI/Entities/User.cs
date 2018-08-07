using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFLManagementAPI.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public DateTime Birthdate { get; set; }
        public int Postcode { get; set; }
        public string Comment { get; set; }
        public DateTime TimestampRegistration { get; set; }

        public virtual ICollection<Presence> Presence { get; set; }
        public virtual ICollection<Card> Cards { get; set; }
    }
}

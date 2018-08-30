using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EFLManagementAPI.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public DateTime Birthdate { get; set; }
        public int Postcode { get; set; }
        public string Comment { get; set; }
        public DateTime TimestampRegistration { get; set; }
        public bool IsAdmin { get; set; }

        public List<Presence> Presence { get; set; }
        public List<Card> Cards { get; set; }
    }
}

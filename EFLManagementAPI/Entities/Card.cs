using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EFLManagementAPI.Entities
{
    public class Card
    {
        [Key]
        public int CardId { get; set; }

        [Required]
        public string CardCode { get; set; }

        [Required]
        public User User { get; set; }

        [Required]
        public DateTime TimestampRegistration { get; set; }
    }
}

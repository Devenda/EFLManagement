using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFLManagementAPI.Models
{
    public class Error
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string InnerException { get; set; }
    }
}

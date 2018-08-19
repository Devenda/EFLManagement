using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFLManagementAPI.Entities
{
    public class Error
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}

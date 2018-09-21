using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFLManagementAPI.Hubs
{
    interface ICardHub
    {
        Task SendMessage(string user, string message);
    }
}

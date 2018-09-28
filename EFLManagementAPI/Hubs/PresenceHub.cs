using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFLManagementAPI.Hubs
{
    public class PresenceHub : Hub
    {
        private IHubContext<PresenceHub> _context;

        public PresenceHub(IHubContext<PresenceHub> context)
        {
            _context = context;
        }

        public async Task SendMessage(int presenceId)
        {
            await _context.Clients.All.SendAsync("ReceiveMessage", presenceId);
        }
    }
}

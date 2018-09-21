using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFLManagementAPI.Hubs
{
    public class CardHub : Hub
    {
        private IHubContext<CardHub> _context;

        public CardHub(IHubContext<CardHub> context)
        {
            _context = context;
        }

        public async Task SendMessage(string code)
        {
            await _context.Clients.All.SendAsync("ReceiveMessage", code);
        }
    }
}

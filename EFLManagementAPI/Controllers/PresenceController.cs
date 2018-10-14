using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFLManagementAPI.Context;
using EFLManagementAPI.Entities;
using EFLManagementAPI.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EFLManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PresenceController : ControllerBase
    {
        private readonly EFLContext _eflContext;
        private readonly PresenceHub _presenceHub;

        public PresenceController(EFLContext eflContext, PresenceHub presenceHub)
        {
            _eflContext = eflContext;
            _presenceHub = presenceHub;
        }

        [HttpPost]
        [Route("test")]
        public async Task<ActionResult> SendTestPresenceScan(string name)
        {
            await _presenceHub.SendNewPresenceReceived(name);

            return StatusCode(200);
        }
    }
}

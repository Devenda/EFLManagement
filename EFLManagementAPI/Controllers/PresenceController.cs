using EFLManagementAPI.Context;
using EFLManagementAPI.Entities;
using EFLManagementAPI.Hubs;
using EFLManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [HttpGet]
        [Route("presences")]
        public ActionResult<IList<Day>> GetPresences(int months)
        {
            var presences = _eflContext.Presence.Where(p => p.TimestampScan >= DateTime.Now.AddMonths(-1 * months))
                                                .GroupBy(p => p.TimestampScan.Date)
                                                .Select(p => new Day { Date = p.Key, Presences = p.Count() })
                                                .ToList();

            return presences;
        }

        [HttpGet]
        [Route("date")]
        public ActionResult<IList<User>> GetPresentUsersForDate(DateTime date)
        {
            var presences = _eflContext.Presence.Where(p => p.TimestampScan.Date == date.Date)
                                                .Select(p => p.User)
                                                .ToList();

            return presences;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFLManagementAPI.Context;
using EFLManagementAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EFLManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PresenceController : ControllerBase
    {
        private readonly EFLContext _eflContext;

        public PresenceController(EFLContext eflContext)
        {
            _eflContext = eflContext;
        }           
    }
}

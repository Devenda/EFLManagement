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

        // GET: api/Presence
        [HttpGet]
        public IEnumerable<Card> Get()
        {
            return _eflContext.Card.ToList();
        }

        // GET: api/Presence/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return _eflContext.Card.First().ToString();
        }

        // POST: api/Presence
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Presence/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

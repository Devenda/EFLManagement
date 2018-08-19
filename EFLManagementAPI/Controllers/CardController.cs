using EFLManagementAPI.Context;
using EFLManagementAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EFLManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly EFLContext _eflContext;

        public CardController(EFLContext eflContext)
        {
            _eflContext = eflContext;
        }

        [HttpGet]
        public ActionResult<IList<Card>> Get()
        {
            return _eflContext.Card.ToList();
        }

        [HttpGet("{id}", Name = "Get")]
        public ActionResult<Card> Get(int id)
        {
            return _eflContext.Card.Where(c => c.CardId == id)
                                   .FirstOrDefault();                                   
        }

        [HttpPost]
        [Route("{id}/link")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult LinkCard(int userId, [FromBody] string cardCode)
        {
            var user = _eflContext.User.Where(u => u.UserId == userId).FirstOrDefault();

            if (user == null) return StatusCode(400, new Error { Message = $"No User exists with id {userId}" });

            //TODO check if card is already linked

            var card = _eflContext.Card.Add(new Card
            {
                CardCode = cardCode,
                User =user,
                TimestampRegistration = DateTime.Now
            });
            _eflContext.SaveChanges();

            return Created("CardForUser", card);
        }
    }
}
using EFLManagementAPI.Context;
using EFLManagementAPI.Entities;
using EFLManagementAPI.Hubs;
using EFLManagementAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFLManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly EFLContext _eflContext;
        private readonly CardHub _cardHub;

        public CardController(EFLContext eflContext, CardHub cardHub)
        {
            _eflContext = eflContext;
            _cardHub = cardHub;
        }

        [HttpGet]
        public ActionResult<IList<Card>> GetAll()
        {
            return _eflContext.Card.ToList();
        }

        [HttpGet("{id}", Name = "Get")]
        public ActionResult<Card> Get(int id)
        {
            return _eflContext.Card.Where(c => c.CardId == id)
                                   .FirstOrDefault();
        }

        [HttpGet]
        [Route("GetAllForUser/{id}")]
        public ActionResult<List<Card>> GetAllForUser(int id)
        {
            return _eflContext.Card.Where(c => c.User.UserId == id)
                                   .ToList();
        }

        [HttpPost]
        [Route("link/{id}")]
        [ProducesResponseType(typeof(Card), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Card> LinkCard(int id, [FromBody] string cardCode)
        {
            var user = _eflContext.User.Where(u => u.UserId == id).FirstOrDefault();

            if (user == null) return StatusCode(400, new Error { Message = $"No User exists with id {id}" });

            //TODO check if card is already linked

            var card = new Card
            {
                CardCode = cardCode,
                User = user,
                TimestampRegistration = DateTime.Now
            };

            _eflContext.Card.Add(card);
            _eflContext.SaveChanges();

            return card;
        }

        [HttpPost]
        [Route("test")]
        public async Task<ActionResult> SendTestCardScan(string code)
        {
            await _cardHub.SendNewCardReceived(code);

            return StatusCode(200);
        }

        [HttpDelete]
        [Route("delete/{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Card> DeleteCard(int id)
        {
            try
            {
                var card = _eflContext.Card.Where(c => c.CardId == id).FirstOrDefault();

                if (card == null) return StatusCode(404, new Error() { Message = $"No Card found with id {id}" });

                _eflContext.Card.Remove(card);
                _eflContext.SaveChanges();

                return card;
            }
            catch (Exception ex)
            {
                string message = $"Something went wrong while deleting card with {id}";
                //_logger.LogError(ex, message);

                return StatusCode(500, new Error() { Message = message, InnerException = ex.InnerException.ToString(), StackTrace = ex.StackTrace });
            }
        }
    }
}
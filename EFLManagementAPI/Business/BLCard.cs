using EFLManagement.Services;
using EFLManagementAPI.Context;
using EFLManagementAPI.Entities;
using EFLManagementAPI.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace EFLManagementAPI.Business
{
    public class BLCard
    {
        private readonly EFLContext _eflContext;
        private readonly CardHub _cardHub;
        private readonly PresenceHub _presenceHub;
        private readonly ILogger<BLCard> _loggerRFIDScanner;

        public BLCard(EFLContext eflContext, CardHub cardHub, PresenceHub presenceHub, ILogger<BLCard> loggerRFIDScanner)
        {
            _eflContext = eflContext;
            _cardHub = cardHub;
            _presenceHub = presenceHub;
            _loggerRFIDScanner = loggerRFIDScanner;
        }
        public async void ProcessCardScan(string cardNumber)
        {
            try
            {
                //Get User
                var user = _eflContext.Card.Include(c => c.User)
                                           .Where(c => c.CardCode == cardNumber)
                                           .FirstOrDefault()?.User;

                if (user == null)
                {
                    _loggerRFIDScanner.LogWarning($"No user found with a card with cardcode: {cardNumber}, sending to frontend for registration.");
                    await _cardHub.SendNewCardReceived(cardNumber);
                    await _presenceHub.SendUnknownCardReceived(cardNumber);

                    return;
                }

                if (user.Name != "Gast")
                {
                    //Check if user already badged in the last hour    
                    var presence4Hours = _eflContext.Presence.Where(p => p.User == user)
                                                             .Where(p => p.TimestampScan >= DateTime.Now.AddHours(-4));
                    if (presence4Hours.Any())
                    {
                        //user already present
                        await _presenceHub.SendNewPresenceReceived($"Welkom terug {user.Name}!");
                        return;
                    }
                }

                //Send presence id to frontend with message.
                string message = "";
                if (DateTime.Now.Hour < 12)
                {
                    message = "Goedemorgen";
                }
                else if (DateTime.Now.Hour < 17)
                {
                    message = "Goedemiddag";
                }
                else
                {
                    message = "Goedeavond";
                }
                await _presenceHub.SendNewPresenceReceived($"{message}, {user.Name}!");

                //Add to db
                _loggerRFIDScanner.LogInformation($"Adding presence for {user.Name}");
                Presence presence = new Presence { TimestampScan = DateTime.Now, User = user };
                _eflContext.Presence.Add(presence);
                _eflContext.SaveChanges();

                _loggerRFIDScanner.LogInformation($"Added new user presence with id {presence.PresenceId}");

            }
            catch (Exception ex)
            {
                _loggerRFIDScanner.LogError(ex, $"Error occured while processing card scan for cardcode: {cardNumber}");
            }
        }
    }
}

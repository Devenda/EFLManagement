using EFLManagementAPI.Context;
using EFLManagementAPI.Entities;
using EFLManagementAPI.Hubs;
using Input_event;
using Input_event.InputDevices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EFLManagement.Services
{
    internal class RFIDScanner : IHostedService
    {
        Keyboard _keyboard;
        private readonly string _eventPath = "/dev/input/event0";

        //https://stackoverflow.com/questions/48368634/how-should-i-inject-a-dbcontext-instance-into-an-ihostedservice
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RFIDScanner> _loggerRFIDScanner;

        private readonly HubLifetimeManager<CardHub> _cardHubManager;
        private readonly HubLifetimeManager<PresenceHub> _presenceHubManager;

        private readonly Dictionary<string, string> _userCardCache;
        private readonly Dictionary<string, DateTime> _presenceCache;

        private List<char> _cardNumberPartial = new List<char>();
        private string _cardNumber;

        readonly Dictionary<Keyboard.InputEventCode, char> buttonValues = new Dictionary<Keyboard.InputEventCode, char>()
        {
            {Keyboard.InputEventCode.KEY_0, '0' },
            {Keyboard.InputEventCode.KEY_1, '1' },
            {Keyboard.InputEventCode.KEY_2, '2' },
            {Keyboard.InputEventCode.KEY_3, '3' },
            {Keyboard.InputEventCode.KEY_4, '4' },
            {Keyboard.InputEventCode.KEY_5, '5' },
            {Keyboard.InputEventCode.KEY_6, '6' },
            {Keyboard.InputEventCode.KEY_7, '7' },
            {Keyboard.InputEventCode.KEY_8, '8' },
            {Keyboard.InputEventCode.KEY_9, '9' },
        };

        public RFIDScanner(IServiceScopeFactory scopeFactory, ILogger<RFIDScanner> loggerRFIDScanner,
            HubLifetimeManager<CardHub> cardHubManager, HubLifetimeManager<PresenceHub> presenceHubManager)
        {
            _scopeFactory = scopeFactory;
            _loggerRFIDScanner = loggerRFIDScanner;
            _cardHubManager = cardHubManager;
            _presenceHubManager = presenceHubManager;

            _userCardCache = new Dictionary<string, string>();
            _presenceCache = new Dictionary<string, DateTime>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _loggerRFIDScanner.LogInformation($"Starting RFIDScanner");

            //_loggerRFIDScanner.LogInformation($"root directory: {Directory.GetCurrentDirectory()}");
            //foreach (var item in Directory.GetFiles(Directory.GetCurrentDirectory()))
            //{
            //    _loggerRFIDScanner.LogInformation($"file: {item}");
            //}

            //foreach (var dir in Directory.GetDirectories(Directory.GetCurrentDirectory()))
            //{
            //    _loggerRFIDScanner.LogInformation($"directory: {dir}");

            //    foreach (var item in Directory.GetFiles(dir))
            //    {
            //        _loggerRFIDScanner.LogInformation($"file: {item}");
            //    }
            //}

            //Pull initial cache
            await RefreshUserCardCacheAsync();

            _keyboard = new Keyboard(new InputEvent(_eventPath));
            _keyboard.ButtonDownEvent += Keyboard_ButtonDownEvent;
            _keyboard.Start();

            _loggerRFIDScanner.LogInformation($"Started RFIDScanner");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _keyboard?.Stop();

            return Task.CompletedTask;
        }

        internal void Keyboard_ButtonDownEvent(object sender, Keyboard.InputEventArgs e)
        {
            //_loggerRFIDScanner.LogInformation("Received keyboard event");
            if (e.Code != Keyboard.InputEventCode.KEY_ENTER)
            {
                //_loggerRFIDScanner.LogInformation($"Button down: {e}");
                _cardNumberPartial.Add(buttonValues[e.Code]);
            }
            else if (e.Code == Keyboard.InputEventCode.KEY_ENTER)
            {
                _cardNumber = string.Concat(_cardNumberPartial);
                _loggerRFIDScanner.LogInformation($"Card scanned with number: {_cardNumber}");
                ProcessCardScan(_cardNumber);
                _cardNumberPartial.Clear();
            }
            else
            {
                _loggerRFIDScanner.LogWarning($"Unsupported key scanned with code: {e.Code}");
            }
        }

        private async void ProcessCardScan(string cardNumber)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    EFLContext _eflContext = scope.ServiceProvider.GetRequiredService<EFLContext>();
                    CardHub _cardHub = scope.ServiceProvider.GetRequiredService<CardHub>();
                    PresenceHub _presenceHub = scope.ServiceProvider.GetRequiredService<PresenceHub>();

                    //Get User
                    var userName = "";
                    if (_userCardCache.ContainsKey(cardNumber))
                    {                        
                        userName = _userCardCache[cardNumber];
                        _loggerRFIDScanner.LogInformation($"Found user {userName} in cache with cardNumber {cardNumber}.");
                    }
                    else
                    {
                        _loggerRFIDScanner.LogInformation($"Could not find user for {cardNumber} in cache, looking in DB.");
                        userName = _eflContext.Card.Include(c => c.User)
                                                   .Where(c => c.CardCode == cardNumber)
                                                   .FirstOrDefault()?.User.Name;
                        
                        if (!string.IsNullOrEmpty(userName))
                        {
                            _loggerRFIDScanner.LogInformation($"Found user {userName} in DB, refreshing cache.");
                            RefreshUserCardCacheAsync();
                        }
                    }

                    if (string.IsNullOrEmpty(userName))
                    {
                        _loggerRFIDScanner.LogWarning($"No user found with a card with cardcode: {cardNumber}, sending to frontend for registration.");
                        await _cardHub.SendNewCardReceived(cardNumber);
                        await _presenceHub.SendUnknownCardReceived(cardNumber);

                        return;
                    }

                    if (userName != "Gast")
                    {
                        //Check if user already badged in the last hour                    
                        if (ManagePresenceForCard(cardNumber) != null)
                        {
                            //user already present
                            await _presenceHub.SendNewPresenceReceived($"Welkom terug {userName}!");
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
                    await _presenceHub.SendNewPresenceReceived($"{message}, {userName}!");

                    //Add to db
                    _loggerRFIDScanner.LogInformation($"Looking for user with cardcode: {cardNumber}.");
                    User user = _eflContext.Card.Include(c => c.User)
                                                   .Where(c => c.CardCode == cardNumber)
                                                   .FirstOrDefault()?.User;
                    _loggerRFIDScanner.LogInformation($"Adding presence for {user.Name}");
                    Presence presence = new Presence { TimestampScan = DateTime.Now, User = user };
                    _eflContext.Presence.Add(presence);
                    _eflContext.SaveChanges();

                    _loggerRFIDScanner.LogInformation($"Added new user presence with id {presence.PresenceId}");
                }
            }
            catch (Exception ex)
            {
                _loggerRFIDScanner.LogError(ex, $"Error occured while processing card scan for cardcode: {cardNumber}");
            }
        }

        private async Task RefreshUserCardCacheAsync()
        {
            _loggerRFIDScanner.LogInformation($"Start refresh cache");

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    EFLContext _eflContext = scope.ServiceProvider.GetRequiredService<EFLContext>();

                    var allUsers = await _eflContext.User.Include(u => u.Cards)
                                                         .ToListAsync();
                    if (!allUsers.Any())
                    {
                        _loggerRFIDScanner.LogWarning($"No users could be found, which is strange...");
                        return;
                    }

                    _userCardCache.Clear();
                    foreach (User user in allUsers)
                    {
                        if (user.Cards.Any())
                        {
                            foreach (Card card in user.Cards)
                            {
                                _userCardCache.Add(card.CardCode, user.Name);
                            }
                        }
                    }

                    _loggerRFIDScanner.LogInformation($"Cache contains:");
                    foreach (var e in _userCardCache)
                    {
                        _loggerRFIDScanner.LogInformation($"Card: {e.Key}, User: {e.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                _loggerRFIDScanner.LogError(ex, $"Error occured while refreshing user cache");
            }
        }

        /// <summary>
        /// Add/Update presence for card
        /// If to recent return datetime
        /// </summary>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        private DateTime? ManagePresenceForCard(string cardCode)
        {
            if (_presenceCache.ContainsKey(cardCode))
            {
                var regTimeStamp = _presenceCache[cardCode];

                if (regTimeStamp < DateTime.Now.AddHours(-4))
                {
                    _presenceCache[cardCode] = DateTime.Now;

                    return null;
                }

                return regTimeStamp;
            }

            _presenceCache.Add(cardCode, DateTime.Now);

            return null;
        }
    }
}

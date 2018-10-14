using EFLManagementAPI.Context;
using EFLManagementAPI.Entities;
using EFLManagementAPI.Hubs;
using Input_event;
using Input_event.InputDevices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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


        public RFIDScanner(IServiceScopeFactory scopeFactory, ILogger<RFIDScanner> loggerRFIDScanner,
            HubLifetimeManager<CardHub> cardHubManager, HubLifetimeManager<PresenceHub> presenceHubManager)
        {
            _scopeFactory = scopeFactory;
            _loggerRFIDScanner = loggerRFIDScanner;
            _cardHubManager = cardHubManager;
            _presenceHubManager = presenceHubManager;
        }

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

        private List<char> _cardNumberPartial = new List<char>();
        private string _cardNumber;

        public Task StartAsync(CancellationToken cancellationToken)
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

            _keyboard = new Keyboard(new InputEvent(_eventPath));
            _keyboard.ButtonDownEvent += Keyboard_ButtonDownEvent;
            _keyboard.Start();

            _loggerRFIDScanner.LogInformation($"Started RFIDScanner");

            return Task.CompletedTask;
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

        private void ProcessCardScan(string cardNumber)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    EFLContext _eflContext = scope.ServiceProvider.GetRequiredService<EFLContext>();

                    var card = _eflContext.Card.Where(c => c.CardCode == cardNumber).FirstOrDefault();
                    if (card == null)
                    {
                        _loggerRFIDScanner.LogWarning($"Unregistered card scanned, will be send to client for registration.");
                        _cardHubManager.SendAllAsync("ReceiveMessage", new object[] { cardNumber });
                        return;
                    }

                    var user = _eflContext.User.Where(u => u.Cards.Contains(card)).First();

                    if (user == null)
                    {
                        _loggerRFIDScanner.LogWarning($"Card found with id:{card.CardId}, but user not, why isn't it linked?");
                        return;
                    }

                    Presence presence = new Presence { TimestampScan = DateTime.Now, User = user };

                    _eflContext.Presence.Add(presence);
                    _eflContext.SaveChanges();

                    //TODO send presence id to frontend.

                    _loggerRFIDScanner.LogInformation($"Added new user presence with id {presence.PresenceId}");
                }
            }
            catch (Exception ex)
            {
                _loggerRFIDScanner.LogError(ex, $"Error occured while processing card scan for cardcode: {cardNumber}");
            }
        }
    }
}

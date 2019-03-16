using EFLManagementAPI.Business;
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

        private readonly BLCard _blCard;

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

        public RFIDScanner(IServiceScopeFactory scopeFactory, ILogger<RFIDScanner> loggerRFIDScanner, BLCard blCard)
        {
            _scopeFactory = scopeFactory;
            _loggerRFIDScanner = loggerRFIDScanner;
            _blCard = blCard;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _loggerRFIDScanner.LogInformation($"Starting RFIDScanner");

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
                _blCard.ProcessCardScan(_cardNumber);
                _cardNumberPartial.Clear();
            }
            else
            {
                _loggerRFIDScanner.LogWarning($"Unsupported key scanned with code: {e.Code}");
            }
        }                   
    }
}

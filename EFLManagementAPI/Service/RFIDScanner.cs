using Input_event;
using Input_event.InputDevices;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Input_Event.Devices.InputDevice;

namespace EFLManagement.Services
{
    internal class RFIDScanner : IHostedService
    {
        Keyboard _keyboard;
        private string _EventPath = "/dev/input/event0";

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
            _keyboard = new Keyboard(new InputEvent(_EventPath));
            _keyboard.ButtonDownEvent += Keyboard_ButtonDownEvent;
            _keyboard.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _keyboard.Stop();

            return Task.CompletedTask;
        }

        internal void Keyboard_ButtonDownEvent(object sender, Keyboard.InputEventArgs e)
        {
            if (e.Code != Keyboard.InputEventCode.KEY_ENTER)
            {
                Console.WriteLine("Button down: {0}", e);
                _cardNumberPartial.Add(buttonValues[e.Code]);
            }
            else if (e.Code == Keyboard.InputEventCode.KEY_ENTER)
            {
                _cardNumber = String.Concat(_cardNumberPartial);
                Console.WriteLine($"Card Number: {_cardNumber}");
                _cardNumberPartial.Clear();
            }
            else
            {
                //TODO TSC: Key not supported
            }
        }
    }
}

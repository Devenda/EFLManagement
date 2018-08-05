using Input_Event;
using Input_Event.Devices;
using Input_event.InputDevices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static Input_event.InputDevices.Keyboard;

namespace Input_event
{
    public class InputEvent
    {
        //Public prop
        public string EventFilePath { get; set; }
        public InputDevice Device { get; set; }

        public InputEvent(string path)
        {
            //TODO TSC: Add path checks,...
            EventFilePath = path;
        }
    }
}

using Input_event;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Input_Event.Devices
{
    abstract public class InputDevice
    {
        public abstract class InputEventArgs<TDeviceInputEventCodes> : EventArgs
        {
            public EventType Type { get; set; }
            public TDeviceInputEventCodes Code { get; set; }
            public uint Value { get; set; }
            
            public override string ToString()
            {
                return $"Type: {Type}; Code: {Code}, Value: {Value}";
            }
        }

        public enum EventType
        {
            EV_SYN = 0x00,
            EV_KEY = 0x01,
            EV_REL = 0x02,
            EV_ABS = 0x03,
            EV_MSC = 0x04,
            EV_SW = 0x05,
            EV_LED = 0x11,
            EV_SND = 0x12,
            EV_REP = 0x14,
            EV_FF = 0x15,
            EV_PWR = 0x16,
            EV_FF_STATUS = 0x17,
            EV_MAX = 0x1f,
            EV_CNT = (EV_MAX + 1),
        }

        internal string _eventFilePath;
        EvDevReader _evDevReader;

        public InputDevice(InputEvent ie)
        {
            _eventFilePath = ie.EventFilePath;
        }

        public abstract void DispatchEvent(ushort code, uint value);
        
        public void Start()
        {
            Task.Run(() =>
            {
                //Console.WriteLine("Starting new reader");
                using (_evDevReader = new EvDevReader(this))
                {
                    _evDevReader.StartReader();
                }
                //Console.WriteLine("Exited reader");
            });
        }

        public void Stop()
        {
            _evDevReader?.StopReader();
        }

    }
}

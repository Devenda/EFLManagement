using Input_Event.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Input_event.InputDevices;

namespace Input_Event
{
    class EvDevReader : IDisposable
    {
        //Example code: https://nanite.co/2017/12/19/handle-gamepad-input-by-reading-from-dev-input-using-csharp-on-the-pine64-running-linux/
        private FileStream _stream;
        private bool _open = false;
        private InputDevice _device;

        public EvDevReader(InputDevice device)
        {
            try
            {
                _stream = new FileStream(device._eventFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch (Exception)
            {
                //TODO: Handle file not found
                //throw;
            }
            _open = true;
            _device = device;
        }        

        public void StartReader()
        {
            byte[] buffer = new byte[24];
            try
            {
                //Console.WriteLine("Starting EvDevReader");
                while (_open)
                {
                    _stream.Read(buffer, 0, buffer.Length);

                    // start after byte 16 to skip timeval since we don't actually need it
                    int offset = 8;
                    ushort type = BitConverter.ToUInt16(new byte[] { buffer[offset], buffer[++offset] }, 0);
                    ushort code = BitConverter.ToUInt16(new byte[] { buffer[++offset], buffer[++offset] }, 0);
                    uint value = BitConverter.ToUInt32(new byte[] { buffer[++offset], buffer[++offset], buffer[++offset], buffer[++offset] }, 0);

                    //Console.WriteLine($"Type: {type}; Code: {code}, Value: {value}");
                    // Dispatch corresponding gamepad event to any subscribed event handlers
                    DispatchEvent(type, code, value);
                }
                //Console.WriteLine("Stopped EvDevReader");
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw ex;
            }
        }

        public void StopReader()
        {
            _open = false;
            _stream.Dispose();
        }

        private void DispatchEvent(ushort type, ushort code, uint value)
        {
            //Console.WriteLine("Dispatching Low Level Event");
            if (_device is Keyboard)
            {
                InputDevice.EventType eventType = (InputDevice.EventType)type;
                //Console.WriteLine($"eventType: {eventType} key {type}");
                switch (eventType)
                {
                    case InputDevice.EventType.EV_SYN:
                        //no error throwing here, just ignor type
                        break;
                    case InputDevice.EventType.EV_ABS:
                        break;
                    case InputDevice.EventType.EV_KEY:
                        // key down and key up events
                        //Console.WriteLine("Caught EV_KEY EVENT");
                        _device.DispatchEvent(code, value);
                        break;
                    case InputDevice.EventType.EV_MSC:
                        break;
                }
            }
        }

        public void Dispose()
        {
            _open = false;

            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
                _stream = null;
            }
        }
    }
}

#region MIT/X11 License

/*
Sharp Input System Library
Copyright © 2007-2019 Michael Cummings

The overall design, and a majority of the core code contained within 
this library is a derivative of the open source Open Input System ( OIS ) , 
which can be found at http://www.sourceforge.net/projects/wgois.  
Many thanks to Phillip Castaneda for maintaining such a high quality project.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

*/

#endregion MIT/X11 License

#region Namespace Declarations

using System;
using System.Collections.Generic;
using System.Reflection;

using Common.Logging;

using MDI = SharpDX.DirectInput;
using SXI = SharpDX.XInput;

#endregion Namespace Declarations

namespace SharpInputSystem.DirectX
{
    /// <summary>
    /// DirectX 9.0c InputManager specialization
    /// </summary>
    internal class DirectXInputManager : InputManager, InputObjectFactory
    {
        #region Fields and Properties

        private static readonly ILog log = LogManager.GetLogger(typeof(DirectXInputManager));

        private readonly Dictionary<String, MDI.CooperativeLevel> _settings = new Dictionary<string, MDI.CooperativeLevel>();
        private readonly List<DeviceInfo> _unusedDevices = new List<DeviceInfo>();
        private readonly MDI.DirectInput directInput = new MDI.DirectInput();
        private int _joystickCount;

        #region keyboardInUse Property

        internal bool keyboardInUse { get; set; }

        #endregion keyboardInUse Property

        #region mouseInUse Property

        private bool _mouseInUse;

        internal bool mouseInUse
        {
            get { return this._mouseInUse; }
            set { this._mouseInUse = value; }
        }

        #endregion keyboardInUse Property

        #region WindowHandle Property

        private IntPtr _hwnd;

        public IntPtr WindowHandle
        {
            get { return this._hwnd; }
        }

        #endregion WindowHandle Property

        public bool HideMouse { get; set; }

        #endregion Fields and Properties

        internal DirectXInputManager()
        {
            RegisterFactory(this);
        }

        public override string InputSystemName
        {
            get { return "DirectX"; }
        }

        protected override void _initialize(ParameterList args)
        {
            // Find the WINDOW parameter
            Parameter parameter = args.Find((p) => { return p.first.ToLower() == "window"; });
            if (parameter != null)
            {
                if (parameter.second is IntPtr) {
                    this._hwnd = (IntPtr)parameter.second;
                }
                else
                    throw new Exception("SharpInputSystem.DirectXInputManger requires a Handle to a window.");
            }

            parameter = args.Find((p) => { return p.first.ToLower() == "w32_mouse_hide"; });
            if (parameter != null)
            {
                if (parameter.second is Boolean)
                    HideMouse = (bool)parameter.second;
            }

            this._settings.Add(typeof(Mouse).Name, 0);
            this._settings.Add(typeof(Keyboard).Name, 0);
            this._settings.Add(typeof(Joystick).Name, 0);

            //Ok, now we have DirectInput, parse whatever extra settings were sent to us
            _parseConfigSettings(args);
            _enumerateDevices();
        }

        private void _enumerateDevices()
        {
            var keyboardInfo = new KeyboardInfo();
            keyboardInfo.Vendor = InputSystemName;
            keyboardInfo.Id = 0;
            this._unusedDevices.Add(keyboardInfo);

            var mouseInfo = new MouseInfo();
            mouseInfo.Vendor = InputSystemName;
            mouseInfo.Id = 0;
            this._unusedDevices.Add(mouseInfo);

            foreach (MDI.DeviceInstance device in this.directInput.GetDevices(MDI.DeviceClass.GameControl, MDI.DeviceEnumerationFlags.AttachedOnly))
            {
                //if ( device.Type == MDI.DeviceType.Joystick || device.Type == MDI.DeviceType.Gamepad ||
                //     device.Type == MDI.DeviceType.FirstPerson || device.Type == MDI.DeviceType.Driving ||
                //     device.Type == MDI.DeviceType.Flight )
                //{
                var joystickInfo = new JoystickInfo();
                joystickInfo.IsXInput = false;
                joystickInfo.ProductGuid = device.ProductGuid;
                joystickInfo.DeviceId = device.InstanceGuid;
                joystickInfo.Vendor = device.ProductName;
                joystickInfo.Id = this._joystickCount++;

                this._unusedDevices.Add(joystickInfo);
                //}
            }

            try
            {
                var controllers = new[] { new SXI.Controller(SXI.UserIndex.One), new SXI.Controller(SXI.UserIndex.Two), new SXI.Controller(SXI.UserIndex.Three), new SXI.Controller(SXI.UserIndex.Four) };
                foreach (var controller in controllers)
                {
                    if (controller.IsConnected)
                    {
                        DirectXJoystick.CheckXInputDevices(_unusedDevices);
                    }
                }
            }
            catch (DllNotFoundException)
            {

            }
        }

        private void _parseConfigSettings(ParameterList args)
        {
            var noCooperativeMode = false;
            var valueMap = new Dictionary<string, MDI.CooperativeLevel>();

            valueMap.Add("CLF_BACKGROUND", MDI.CooperativeLevel.Background);
            valueMap.Add("CLF_FOREGROUND", MDI.CooperativeLevel.Foreground);
            valueMap.Add("CLF_EXCLUSIVE", MDI.CooperativeLevel.Exclusive);
            valueMap.Add("CLF_NONEXCLUSIVE", MDI.CooperativeLevel.NonExclusive);
            valueMap.Add("CLF_NOWINDOWSKEY", MDI.CooperativeLevel.NoWinKey);

            foreach (Parameter p in args)
            {
                switch (p.first.ToUpper())
                {
                    case "W32_MOUSE":
                        this._settings[typeof(Mouse).Name] |= valueMap[p.second.ToString().ToUpper()];
                        break;
                    case "W32_KEYBOARD":
                        this._settings[typeof(Keyboard).Name] |= valueMap[p.second.ToString().ToUpper()];
                        break;
                    case "W32_JOYSTICK":
                        this._settings[typeof(Joystick).Name] |= valueMap[p.second.ToString().ToUpper()];
                        break;
                    case "W32_NO_COOP":
                        noCooperativeMode = true;
                        break;
                    default:
                        break;
                }
            }

            if (noCooperativeMode)
            {
                this._settings[typeof(Mouse).Name] = 0;
                this._settings[typeof(Keyboard).Name] = 0;
                this._settings[typeof(Joystick).Name] = 0;
            }
            else
            {
                if (this._settings[typeof(Mouse).Name] == 0)
                    this._settings[typeof(Mouse).Name] = MDI.CooperativeLevel.Exclusive | MDI.CooperativeLevel.Foreground;
                if (this._settings[typeof(Keyboard).Name] == 0)
                    this._settings[typeof(Keyboard).Name] = MDI.CooperativeLevel.NonExclusive | MDI.CooperativeLevel.Background;
                if (this._settings[typeof(Joystick).Name] == 0)
                    this._settings[typeof(Joystick).Name] = MDI.CooperativeLevel.Exclusive | MDI.CooperativeLevel.Foreground;
            }
        }

        internal DeviceInfo PeekDevice<T>() where T : InputObject
        {
            string devType = typeof(T).Name + "Info";

            foreach (DeviceInfo device in this._unusedDevices)
            {
                if (devType == device.GetType().Name)
                    return device;
            }

            return null;
        }

        internal DeviceInfo CaptureDevice<T>() where T : InputObject
        {
            string devType = typeof(T).Name + "Info";

            foreach (DeviceInfo device in this._unusedDevices)
            {
                if (devType == device.GetType().Name)
                {
                    this._unusedDevices.Remove(device);
                    return device;
                }
            }

            return null;
        }

        internal void ReleaseDevice<T>(DeviceInfo device) where T : InputObject
        {
            this._unusedDevices.Add(device);
        }

        #region InputObjectFactory Implementation

        IEnumerable<KeyValuePair<Type, string>> InputObjectFactory.FreeDevices
        {
            get
            {
                var freeDevices = new List<KeyValuePair<Type, string>>();
                foreach (DeviceInfo dev in this._unusedDevices)
                {
                    if (dev.GetType() == typeof(KeyboardInfo) && !keyboardInUse)
                        freeDevices.Add(new KeyValuePair<Type, string>(typeof(Keyboard), InputSystemName));

                    if (dev.GetType() == typeof(KeyboardInfo) && !this._mouseInUse)
                        freeDevices.Add(new KeyValuePair<Type, string>(typeof(Mouse), InputSystemName));

                    if (dev.GetType() == typeof(JoystickInfo))
                        freeDevices.Add(new KeyValuePair<Type, string>(typeof(Joystick), dev.Vendor));
                }
                return freeDevices;
            }
        }

        int InputObjectFactory.DeviceCount<T>()
        {
            if (typeof(T) == typeof(Keyboard))
                return 1;
            if (typeof(T) == typeof(Mouse))
                return 1;
            if (typeof(T) == typeof(Joystick))
                return this._joystickCount;
            return 0;
        }

        int InputObjectFactory.FreeDeviceCount<T>()
        {
            string devType = typeof(T).Name + "Info";
            int deviceCount = 0;
            foreach (DeviceInfo device in this._unusedDevices)
            {
                if (devType == device.GetType().Name)
                    deviceCount++;
            }
            return deviceCount;
        }

        bool InputObjectFactory.VendorExists<T>(string vendor)
        {
            if (typeof(T) == typeof(Keyboard) || typeof(T) == typeof(Mouse) || vendor.ToLower() == InputSystemName.ToLower())
                return true;
            else
            {
                if (typeof(T) == typeof(Joystick))
                {
                    foreach (DeviceInfo dev in this._unusedDevices)
                    {
                        if (dev.GetType() == typeof(JoystickInfo))
                        {
                            var joy = (JoystickInfo)dev;
                            if (joy.Vendor.ToLower() == vendor.ToLower())
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        InputObject InputObjectFactory.CreateInputObject<T>(InputManager creator, bool bufferMode, string vendor)
        {
            string typeName = InputSystemName + typeof(T).Name;
            string name = GetType().FullName.Remove(GetType().FullName.LastIndexOf(".") + 1);
            Type objectType = Assembly.GetExecutingAssembly().GetType(name + typeName);
            T obj = null;

            var bindingFlags = BindingFlags.CreateInstance;
            try
            {
                obj = (T)objectType.InvokeMember(typeName,
                                                     bindingFlags,
                                                     null,
                                                     null,
                                                     new object[] { this, this.directInput, bufferMode, this._settings[typeof(T).Name] });
            }
            catch (Exception ex)
            {
                log.Error("Cannot create requested device.", ex);
            }
            return obj;
        }

        void InputObjectFactory.DestroyInputObject(InputObject obj)
        {
            obj.Dispose();
        }

        #endregion InputObjectFactory Implementation
    }
}

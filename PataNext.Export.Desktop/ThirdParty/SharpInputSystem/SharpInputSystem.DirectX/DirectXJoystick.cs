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

using MDI = SharpDX.DirectInput;
using SXI = SharpDX.XInput;

#endregion Namespace Declarations

namespace SharpInputSystem.DirectX
{
    ///<summary>
    ///
    /// </summary>
    internal class DirectXJoystick : Joystick
    {
        private const int BufferSize = 124;

        private readonly MDI.CooperativeLevel _coopSettings;
        private readonly Guid _deviceGuid;
        private readonly JoystickInfo _joyInfo;
        private Dictionary<int, int> _axisMapping = new Dictionary<int, int>();
        //private int _axisNumber;
        private MDI.DirectInput _directInput;
        private DirectXForceFeedback _forceFeedback;
        private SharpDX.DirectInput.Joystick _joystick;

        private IntPtr _window;

        public DirectXJoystick(InputManager creator, MDI.DirectInput device, bool buffered, MDI.CooperativeLevel coopSettings)
        {
            Creator = creator;
            this._directInput = device;
            IsBuffered = buffered;
            this._coopSettings = coopSettings;
            Type = InputType.Joystick;
            EventListener = null;

            this._joyInfo = (JoystickInfo)((DirectXInputManager)Creator).CaptureDevice<Joystick>();

            if (this._joyInfo == null)
                throw new Exception("No devices match requested type.");

            this._deviceGuid = this._joyInfo.DeviceId;
            Vendor = this._joyInfo.Vendor;
            DeviceID = this._joyInfo.Id.ToString();
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            if (!IsDisposed)
            {
                if (disposeManagedResources)
                {
                    // Dispose managed resources.
                }

                // There are no unmanaged resources to release, but
                // if we add them, they need to be released here.
                if (this._joystick != null)
                {
                    try
                    {
                        this._joystick.Unacquire();
                    }
                    finally
                    {
                        this._joystick.Dispose();
                        this._joystick = null;
                        this._directInput = null;
                        this._forceFeedback = null;
                    }

                    ((DirectXInputManager)Creator).ReleaseDevice<Joystick>(this._joyInfo);
                }
            }
            IsDisposed = true;

            // If it is available, make the call to the
            // base class's Dispose(Boolean) method
            base.Dispose(disposeManagedResources);
        }

        protected void Enumerate()
        {
            if (_joyInfo.IsXInput)
            {
                this.PovCount = 1;
                JoystickState.Axis.AddRange(new List<Axis>() { new Axis(), new Axis(), new Axis(), new Axis(), new Axis(), new Axis() });
            }
            else
            {
                //We can check force feedback here too
                MDI.Capabilities joystickCapabilities;

                joystickCapabilities = _joystick.Capabilities;
                this.AxisCount = (short)joystickCapabilities.AxeCount;
                this.ButtonCount = (short)joystickCapabilities.ButtonCount;
                this.PovCount = (short)joystickCapabilities.PovCount;

                for (int axis = 0; axis < AxisCount; axis++)
                    JoystickState.Axis.Add(new Axis());

                //_axisNumber = 0;
                _axisMapping.Clear();

                //TODO: Enumerate Force Feedback (if any)
                /*
                foreach ( var effect in _joystick.GetEffects() )
                {
                    if ( this._forceFeedback == null)
                        this._forceFeedback = new DirectXForceFeedback( this._joystick, joystickCapabilities );
                    this._forceFeedback.SupportedEffects.AddEffectSupport( effect );
                }
                */

                //TODO: Enumerate and set axis constraints (and check FF Axes)
                /*
                foreach ( var doi in _joystick.GetObjects() )
                {
                    if ( doi.ObjectType == MDI.ObjectGuid.Slider)
                    {
                        this._sliders++;
                        this.AxisCount--;
                    }

                    if ( doi.ObjectType != MDI.ObjectGuid.Slider)
                    {
                        this._axisNumber++;
                    }

                    if ( ( doi.Usage && MDI.DeviceObjectTypeFlags.ForceFeedbackActuator ) != 0 )
                    {
                        if (this._forceFeedback != null)
                            this._forceFeedback.AddAxis( );
                    }
                }
                */
            }
        }

        internal static void CheckXInputDevices(List<DeviceInfo> unusedDevices)
        {
            if (unusedDevices.Count == 0)
                return;
            /*

            int xDeviceIndex = 0;

            //dictionary object to hold the values
            Dictionary<string, string> driveInfo = new Dictionary<string, string>();
            //create our WMI searcher
            var searcher = new ManagementObjectSearcher(@"select * from Win32_PNPEntity");
            //now loop through all the item found with the query
            foreach (var obj in searcher.Get())
            {
                
                var deviceId = (string)obj[ "DeviceID" ];
                string vId, pId;
                if (deviceId.Contains("IG_"))
                {
                    vId = deviceId.Substring( deviceId.IndexOf( "VID_", System.StringComparison.Ordinal ) + 4, 4 );
                    pId = deviceId.Substring( deviceId.IndexOf( "PID_", System.StringComparison.Ordinal ) + 4, 4 );

                    string vpId = (pId + vId).ToLower();
                    foreach ( var info in unusedDevices )
                    {
                        var joyInfo = info as JoystickInfo;
                        if ( joyInfo != null)
                        {
                            if ( joyInfo.IsXInput == false && vpId == joyInfo.ProductGuid.ToString().Substring(0,8).ToLower() )
                            {
                                joyInfo.IsXInput = true;
                                joyInfo.XInputDevice = xDeviceIndex++;
                            }
                        }
                    }
                }
            }
            */
        }

        private bool DoButtonClick(int offset, MDI.JoystickUpdate entry)
        {
            var eventArgs = new JoystickEventArgs(this, JoystickState);
            if ((entry.Value & 0x80) != 0)
            {
                JoystickState.Buttons |= offset;
                if (IsBuffered && EventListener != null)
                    return EventListener.ButtonPressed(eventArgs, 0);
            }
            else
            {
                JoystickState.Buttons &= ~offset;
                if (IsBuffered && EventListener != null)
                    return EventListener.ButtonReleased(eventArgs, 0);
            }
            return true;
        }

        private bool ChangePOV(int button, MDI.JoystickUpdate entry)
        {
            return true;
        }

        public override void Capture()
        {
            if (_joyInfo.IsXInput)
            {
                CaptureXInput();
                return;
            }

            MDI.JoystickUpdate[] bufferedData = null;
            bufferedData = this._joystick.GetBufferedData();
            if (bufferedData == null)
            {
                this._joystick.Poll();
                bufferedData = this._joystick.GetBufferedData();
                if (bufferedData == null)
                    return;
            }

            bool[] axisMoved = {
                                   false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false,
                                   false, false, false, false, false, false, false, false
                               };
            bool[] sliderMoved = { false, false, false, false };

            foreach (var entry in bufferedData)
            {
                switch (entry.Offset)
                {
                    /* sliders */
                    case MDI.JoystickOffset.Sliders1:
                        sliderMoved[0] = true;
                        JoystickState.Sliders[0].X = entry.Value;
                        break;
                    case MDI.JoystickOffset.Sliders0:
                        sliderMoved[0] = true;
                        JoystickState.Sliders[0].Y = entry.Value;
                        break;
                    case MDI.JoystickOffset.PointOfViewControllers0:
                        if (ChangePOV(0, entry))
                            return;
                        break;
                    case MDI.JoystickOffset.PointOfViewControllers1:
                        if (ChangePOV(1, entry))
                            return;
                        break;
                    case MDI.JoystickOffset.PointOfViewControllers2:
                        if (ChangePOV(2, entry))
                            return;
                        break;
                    case MDI.JoystickOffset.PointOfViewControllers3:
                        if (ChangePOV(3, entry))
                            return;
                        break;
                    case MDI.JoystickOffset.VelocitySliders0:
                        sliderMoved[1] = true;
                        JoystickState.Sliders[1].X = entry.Value;
                        break;
                    case MDI.JoystickOffset.VelocitySliders1:
                        sliderMoved[1] = true;
                        JoystickState.Sliders[1].X = entry.Value;
                        break;
                    case MDI.JoystickOffset.AccelerationSliders0:
                        sliderMoved[0] = true;
                        JoystickState.Sliders[2].X = entry.Value;
                        break;
                    case MDI.JoystickOffset.AccelerationSliders1:
                        sliderMoved[0] = true;
                        JoystickState.Sliders[2].X = entry.Value;
                        break;
                    case MDI.JoystickOffset.ForceSliders0:
                        sliderMoved[0] = true;
                        JoystickState.Sliders[3].X = entry.Value;
                        break;
                    case MDI.JoystickOffset.ForceSliders1:
                        sliderMoved[0] = true;
                        JoystickState.Sliders[3].X = entry.Value;
                        break;
                    default:
                        if (entry.Offset >= MDI.JoystickOffset.Buttons0 && entry.Offset < MDI.JoystickOffset.Buttons127)
                        {
                            if (!DoButtonClick((int)entry.Offset - (int)MDI.JoystickOffset.Buttons0, entry))
                                return;
                        }
                        else { }

                        break;
                }
            }

            //Check to see if any of the axes values have changed.. if so send events
            if ((IsBuffered == true) && (EventListener != null) && bufferedData.Length > 0)
            {
                JoystickEventArgs temp = new JoystickEventArgs(this, JoystickState);

                //Update axes
                for (int i = 0; i < axisMoved.Length; i++)
                {
                    if (axisMoved[i])
                    {
                        if (EventListener.AxisMoved(temp, i) == false)
                            return;
                    }
                }

                //Now update sliders
                for (int i = 0; i < 4; i++)
                {
                    if (sliderMoved[i])
                    {
                        if (EventListener.SliderMoved(temp, i) == false)
                            return;
                    }
                }
            }
        }

        private void CaptureXInput()
        {
            var controller = new SXI.Controller((SXI.UserIndex)_joyInfo.XInputDevice);
            var inputState = controller.GetState();

            bool[] axisMoved = { false, false, false, false, false, false, false, false };

            //AXIS
            axisMoved[0] = GetAxisMovement(JoystickState.Axis[0], -inputState.Gamepad.LeftThumbY);
            axisMoved[1] = GetAxisMovement(JoystickState.Axis[1], inputState.Gamepad.LeftThumbX);
            axisMoved[2] = GetAxisMovement(JoystickState.Axis[2], -inputState.Gamepad.RightThumbY);
            axisMoved[3] = GetAxisMovement(JoystickState.Axis[3], inputState.Gamepad.RightThumbX);
            axisMoved[4] = GetAxisMovement(JoystickState.Axis[4], inputState.Gamepad.LeftTrigger * 129 < Joystick.Max_Axis ? inputState.Gamepad.LeftTrigger * 129 : Joystick.Max_Axis);
            axisMoved[5] = GetAxisMovement(JoystickState.Axis[5], inputState.Gamepad.RightTrigger * 129 < Joystick.Max_Axis ? inputState.Gamepad.RightTrigger * 129 : Joystick.Max_Axis);

            //POV
            Pov.Position previousPov = JoystickState.Povs[0].Direction;
            Pov.Position pov = Pov.Position.Centered;
            if ((inputState.Gamepad.Buttons & SXI.GamepadButtonFlags.DPadUp) != 0)
                pov |= Pov.Position.North;
            else if ((inputState.Gamepad.Buttons & SXI.GamepadButtonFlags.DPadDown) != 0)
                pov |= Pov.Position.South;
            if ((inputState.Gamepad.Buttons & SXI.GamepadButtonFlags.DPadLeft) != 0)
                pov |= Pov.Position.West;
            else if ((inputState.Gamepad.Buttons & SXI.GamepadButtonFlags.DPadRight) != 0)
                pov |= Pov.Position.East;
            JoystickState.Povs[0].Direction = pov;

            //BUTTONS
            // Skip the first 4 as they are the DPad.
            var previousButtons = JoystickState.Buttons;
            for (int i = 0; i < 12; i++)
            {
                if (((int)inputState.Gamepad.Buttons & (1 << (i + 4))) != 0)
                {
                    JoystickState.Buttons |= 1 << (i + 4);
                }
                else
                {
                    JoystickState.Buttons &= ~(1 << (i + 4));
                }
            }

            //Send Events
            if (IsBuffered && EventListener != null)
            {
                var joystickEvent = new JoystickEventArgs(this, JoystickState);

                // Axes
                for (int index = 0; index < axisMoved.Length; index++)
                {
                    if (axisMoved[index] == true && EventListener.AxisMoved(joystickEvent, index))
                        return;
                }

                //POV
                if (previousPov != pov && !EventListener.PovMoved(joystickEvent, 0))
                    return;

                //Buttons
                for (int i = 4; i < 16; i++)
                {
                    if (((previousButtons & (1 << i)) == 0) && JoystickState.IsButtonDown(i))
                    {
                        if (!EventListener.ButtonPressed(joystickEvent, i))
                            return;
                    }
                    else if (((previousButtons & (1 << i)) != 0) && !JoystickState.IsButtonDown(i))
                    {
                        if (!EventListener.ButtonReleased(joystickEvent, i))
                            return;
                    }
                }
            }

        }

        private bool GetAxisMovement(Axis axis, int value)
        {
            axis.Relative = value - axis.Absolute;
            axis.Absolute = value;
            return axis.Relative != 0;
        }

        protected override void Initialize()
        {
            if (_joyInfo.IsXInput)
                Enumerate();
            else
            {
                JoystickState.Axis.Clear();

                this._joystick = new SharpDX.DirectInput.Joystick(this._directInput, this._deviceGuid);

                this._window = ((DirectXInputManager)Creator).WindowHandle;

                this._joystick.SetCooperativeLevel(this._window, this._coopSettings);

                if (IsBuffered)
                    this._joystick.Properties.BufferSize = BufferSize;

                //Enumerate all axes/buttons/sliders/etc before aquiring
                Enumerate();

                JoystickState.Clear();

                Capture();
            }
        }

        public override IInputObjectInterface QueryInterface<T>()
        {
            if (typeof(T) == typeof(ForceFeedback))
                return this._forceFeedback;
            return base.QueryInterface<T>();
        }
    }
}

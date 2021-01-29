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
using System.Runtime.InteropServices;

using Common.Logging;

using MDI = SharpDX.DirectInput;

#endregion Namespace Declarations

namespace SharpInputSystem.DirectX
{
    internal class DirectXMouse : Mouse
    {
        #region Nested type: POINT

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public readonly int X;
            public readonly int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        #endregion

        #region Fields and Properties

        private const int BufferSize = 64;
        private static readonly ILog Log = LogManager.GetLogger(typeof(DirectXMouse));

        private readonly MDI.CooperativeLevel _coopSettings;
        private readonly MDI.DirectInput _directInput;
        private readonly bool _hideMouse;
        private readonly MouseInfo _msInfo;
        private MDI.Mouse _mouse;
        private IntPtr _window;

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        #endregion Fields and Properties

        #region Construction and Destruction

        public DirectXMouse(InputManager creator, MDI.DirectInput device, bool buffered, MDI.CooperativeLevel coopSettings)
        {
            Creator = creator;
            this._directInput = device;
            IsBuffered = buffered;
            this._coopSettings = coopSettings;
            Type = InputType.Mouse;
            EventListener = null;

            this._msInfo = (MouseInfo)((DirectXInputManager)Creator).CaptureDevice<Mouse>();

            if (this._msInfo == null)
                throw new Exception("No devices match requested type.");

            this._hideMouse = ((DirectXInputManager)creator).HideMouse;

            Log.Debug("DirectXMouse device created.");
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            if (!IsDisposed)
            {
                if (disposeManagedResources)
                {
                    // Dispose managed resources.

                    if (this._mouse != null)
                    {
                        try
                        {
                            this._mouse.Unacquire();
                        }
                        catch
                        {
                            // NOTE : This is intentional
                        }

                        finally
                        {
                            this._mouse.Dispose();
                            this._mouse = null;
                        }
                    }


                    ((DirectXInputManager)Creator).ReleaseDevice<Mouse>(this._msInfo);
                }
                // There are no unmanaged resources to release, but
                // if we add them, they need to be released here.

                Log.Debug("DirectXMouse device disposed.");
            }

            // If it is available, make the call to the
            // base class's Dispose(Boolean) method
            base.Dispose(disposeManagedResources);
        }

        #endregion Construction and Destruction

        #region Methods

        private bool _doMouseClick(int mouseButton, MDI.MouseUpdate bufferedData)
        {
            if (((bufferedData.Value & 0x80) != 0) && (MouseState.Buttons & (1 << mouseButton)) == 0)
            {
                MouseState.Buttons |= 1 << mouseButton; //turn the bit flag on
                if (EventListener != null && IsBuffered)
                    return EventListener.MousePressed(new MouseEventArgs(this, MouseState), (MouseButtonID)mouseButton);
            }
            else if (((bufferedData.Value & 0x80) == 0) && (MouseState.Buttons & (1 << mouseButton)) != 0)
            {
                MouseState.Buttons &= ~(1 << mouseButton); //turn the bit flag off
                if (EventListener != null && IsBuffered)
                    return EventListener.MouseReleased(new MouseEventArgs(this, MouseState), (MouseButtonID)mouseButton);
            }

            return true;
        }

        private void hide(bool hidePointer)
        {
            if (hidePointer)
                SetCursor(IntPtr.Zero);
            ShowCursor(!hidePointer);
        }

        #endregion Methods

        #region Mouse Implementation

        public override void Capture()
        {
            // Clear Relative movement
            MouseState.X.Relative = MouseState.Y.Relative = MouseState.Z.Relative = 0;

            MDI.MouseUpdate[] bufferedData = this._mouse.GetBufferedData();
            if (bufferedData == null)
            {
                this._mouse.Acquire();

                bufferedData = this._mouse.GetBufferedData();
                if (bufferedData == null)
                    return;
            }
            bool axesMoved = false;

            //Accumulate all axis movements for one axesMove message..
            //Buttons are fired off as they are found
            foreach (MDI.MouseUpdate packet in bufferedData)
            {
                switch (packet.Offset)
                {
                    case MDI.MouseOffset.Buttons0:
                        if (!_doMouseClick(0, packet))
                            return;
                        break;
                    case MDI.MouseOffset.Buttons1:
                        if (!_doMouseClick(1, packet))
                            return;
                        break;
                    case MDI.MouseOffset.Buttons2:
                        if (!_doMouseClick(2, packet))
                            return;
                        break;
                    case MDI.MouseOffset.Buttons3:
                        if (!_doMouseClick(3, packet))
                            return;
                        break;
                    case MDI.MouseOffset.Buttons4:
                        if (!_doMouseClick(4, packet))
                            return;
                        break;
                    case MDI.MouseOffset.Buttons5:
                        if (!_doMouseClick(5, packet))
                            return;
                        break;
                    case MDI.MouseOffset.Buttons6:
                        if (!_doMouseClick(6, packet))
                            return;
                        break;
                    case MDI.MouseOffset.Buttons7:
                        if (!_doMouseClick(7, packet))
                            return;
                        break;
                    case MDI.MouseOffset.X:
                        MouseState.X.Relative = packet.Value;
                        axesMoved = true;
                        break;
                    case MDI.MouseOffset.Y:
                        MouseState.Y.Relative = packet.Value;
                        axesMoved = true;
                        break;
                    case MDI.MouseOffset.Z:
                        MouseState.Z.Relative = packet.Value;
                        axesMoved = true;
                        break;
                }
            }

            if (axesMoved)
            {
                if ((this._coopSettings & MDI.CooperativeLevel.NonExclusive) == MDI.CooperativeLevel.NonExclusive)
                {
                    //DirectInput provides us with meaningless values, so correct that
                    POINT point;
                    GetCursorPos(out point);
                    ScreenToClient(this._window, ref point);
                    MouseState.X.Absolute = point.X;
                    MouseState.Y.Absolute = point.Y;
                }
                else
                {
                    MouseState.X.Absolute += MouseState.X.Relative;
                    MouseState.Y.Absolute += MouseState.Y.Relative;
                }
                MouseState.Z.Absolute += MouseState.Z.Relative;

                //Clip values to window
                if (MouseState.X.Absolute < 0)
                    MouseState.X.Absolute = 0;
                else if (MouseState.X.Absolute > MouseState.Width)
                    MouseState.X.Absolute = MouseState.Width;
                if (MouseState.Y.Absolute < 0)
                    MouseState.Y.Absolute = 0;
                else if (MouseState.Y.Absolute > MouseState.Height)
                    MouseState.Y.Absolute = MouseState.Height;

                //Do the move
                if (EventListener != null && IsBuffered)
                    EventListener.MouseMoved(new MouseEventArgs(this, MouseState));
            }
        }

        protected override void Initialize()
        {
            MouseState.Clear();

            this._mouse = new SharpDX.DirectInput.Mouse(this._directInput);

            this._mouse.Properties.AxisMode = MDI.DeviceAxisMode.Relative;

            this._window = ((DirectXInputManager)Creator).WindowHandle;

            if (this._coopSettings != 0)
                this._mouse.SetCooperativeLevel(this._window, this._coopSettings);

            if (IsBuffered)
                this._mouse.Properties.BufferSize = BufferSize;

            try
            {
                this._mouse.Acquire();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to acquire mouse using DirectInput.", e);
            }

            hide(this._hideMouse);
        }

        #endregion Mouse Implementation
    }
}

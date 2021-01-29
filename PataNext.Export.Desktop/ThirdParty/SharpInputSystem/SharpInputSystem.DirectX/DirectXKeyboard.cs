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

#region tNamespace Declarations

using System;
using Common.Logging;
using SharpDX.DirectInput;

#endregion Namespace Declarations

namespace SharpInputSystem.DirectX
{
    internal class DirectXKeyboard : Keyboard
    {
        #region Fields and Properties

        private const           int  BufferSize = 17;
        private static readonly ILog log        = LogManager.GetLogger(typeof(DirectXKeyboard));

        private readonly CooperativeLevel _coopSettings;
        private readonly DirectInput      _directInput;
        private readonly KeyboardInfo     _kbInfo;

        private readonly int[]                        _keyboardState = new int[256];
        private          SharpDX.DirectInput.Keyboard _keyboard;

        #endregion Fields and Properties

        #region Construction and Destruction

        public DirectXKeyboard(InputManager creator, DirectInput directInput, bool buffered, CooperativeLevel coopSettings)
        {
            Creator            = creator;
            this._directInput  = directInput;
            IsBuffered         = buffered;
            this._coopSettings = coopSettings;
            Type               = InputType.Keyboard;
            EventListener      = null;

            this._kbInfo = (KeyboardInfo) ((DirectXInputManager) Creator).CaptureDevice<Keyboard>();

            if (this._kbInfo == null)
                throw new Exception("No devices match requested type.");

            log.Debug("DirectXKeyboard device created.");
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
                try
                {
                    this._keyboard.Unacquire();
                }
                catch
                {
                    // NOTE : This is intentional
                }
                finally
                {
                    this._keyboard.Dispose();
                    this._keyboard = null;
                }

                ((DirectXInputManager) Creator).ReleaseDevice<Keyboard>(this._kbInfo);

                log.Debug("DirectXKeyboard device disposed.");
            }

            // If it is available, make the call to the
            // base class's Dispose(Boolean) method
            base.Dispose(disposeManagedResources);
        }

        #endregion Construction and Destruction

        #region Methods

        private void Read()
        {
            KeyboardState state = this._keyboard.GetCurrentState();
            for (int i = 0; i < 256; i++)
                this._keyboardState[i] = state.IsPressed((Key) i) ? i : 0;

            //Set Shift, Ctrl, Alt
            shiftState = 0;
            if (IsKeyDown(KeyCode.Key_LCONTROL) || IsKeyDown(KeyCode.Key_RCONTROL))
                shiftState |= ShiftState.Ctrl;
            if (IsKeyDown(KeyCode.Key_LSHIFT) || IsKeyDown(KeyCode.Key_RSHIFT))
                shiftState |= ShiftState.Shift;
            if (IsKeyDown(KeyCode.Key_LMENU) || IsKeyDown(KeyCode.Key_RMENU))
                shiftState |= ShiftState.Alt;
        }

        private void ReadBuffered()
        {
            // grab the collection of buffered data
            KeyboardUpdate[] bufferedData = this._keyboard.GetBufferedData();

            // please tell me why this would ever come back null, rather than an empty collection...
            if (bufferedData == null)
                return;
            foreach (KeyboardUpdate packet in bufferedData)
            {
                bool ret = true;

                //foreach (MDI.Key key in packet.PressedKeys)
                //{
                KeyCode keyCode = Convert(packet.Key);

                //Store result in our keyBuffer too
                this._keyboardState[(int) packet.Key] = 1;

                if (packet.IsPressed)
                {
                    if (packet.Key == Key.RightControl || packet.Key == Key.LeftControl)
                        shiftState |= ShiftState.Ctrl;
                    if (packet.Key == Key.RightAlt || packet.Key == Key.LeftAlt)
                        shiftState |= ShiftState.Alt;
                    if (packet.Key == Key.RightShift || packet.Key == Key.LeftShift)
                        shiftState |= ShiftState.Shift;

                    if (EventListener != null)
                        ret = EventListener.KeyPressed(new KeyEventArgs(this, keyCode, 0));
                    if (ret == false)
                        break;
                }
                else
                {
                    if (packet.Key == Key.RightControl || packet.Key == Key.LeftControl)
                        shiftState &= ~ShiftState.Ctrl;
                    if (packet.Key == Key.RightAlt || packet.Key == Key.LeftAlt)
                        shiftState &= ~ShiftState.Alt;
                    if (packet.Key == Key.RightShift || packet.Key == Key.LeftShift)
                        shiftState &= ~ShiftState.Shift;


                    if (EventListener != null)
                        ret = EventListener.KeyReleased(new KeyEventArgs(this, keyCode, 0));
                    if (ret == false)
                        break;
                }
            }
        }

        private KeyCode Convert(Key key)
        {
            return key switch
            {
                Key.A => KeyCode.Key_A,
                Key.AbntC1 => KeyCode.Key_ABNT_C1,
                Key.AbntC2 => KeyCode.Key_ABNT_C2,
                Key.Apostrophe => KeyCode.Key_APOSTROPHE,
                Key.Applications => KeyCode.Key_APPS,
                Key.AT => KeyCode.Key_AT,
                Key.AX => KeyCode.Key_AX,
                Key.B => KeyCode.Key_B,
                Key.Backslash => KeyCode.Key_BACKSLASH,
                Key.Back => KeyCode.Key_BACK,
                Key.C => KeyCode.Key_C,
                Key.Calculator => KeyCode.Key_CALCULATOR,
                Key.Capital => KeyCode.Key_CAPITAL,
                Key.Colon => KeyCode.Key_COLON,
                Key.Comma => KeyCode.Key_COMMA,
                Key.Convert => KeyCode.Key_CONVERT,
                Key.D => KeyCode.Key_D,
                Key.D0 => KeyCode.Key_0,
                Key.D1 => KeyCode.Key_1,
                Key.D2 => KeyCode.Key_2,
                Key.D3 => KeyCode.Key_3,
                Key.D4 => KeyCode.Key_4,
                Key.D5 => KeyCode.Key_5,
                Key.D6 => KeyCode.Key_6,
                Key.D7 => KeyCode.Key_7,
                Key.D8 => KeyCode.Key_8,
                Key.D9 => KeyCode.Key_9,
                Key.Delete => KeyCode.Key_DELETE,
                Key.Down => KeyCode.Key_DOWN,
                Key.E => KeyCode.Key_E,
                Key.End => KeyCode.Key_END,
                Key.Equals => KeyCode.Key_EQUALS,
                Key.Escape => KeyCode.Key_ESCAPE,
                Key.F => KeyCode.Key_F,
                Key.F1 => KeyCode.Key_F1,
                Key.F2 => KeyCode.Key_F2,
                Key.F3 => KeyCode.Key_F3,
                Key.F4 => KeyCode.Key_F4,
                Key.F5 => KeyCode.Key_F5,
                Key.F6 => KeyCode.Key_F6,
                Key.F7 => KeyCode.Key_F7,
                Key.F8 => KeyCode.Key_F8,
                Key.F9 => KeyCode.Key_F9,
                Key.F10 => KeyCode.Key_F10,
                Key.F11 => KeyCode.Key_F11,
                Key.F12 => KeyCode.Key_F12,
                Key.F13 => KeyCode.Key_F13,
                Key.F14 => KeyCode.Key_F14,
                Key.F15 => KeyCode.Key_F15,
                Key.G => KeyCode.Key_G,
                Key.Grave => KeyCode.Key_GRAVE,
                Key.H => KeyCode.Key_H,
                Key.Home => KeyCode.Key_HOME,
                Key.I => KeyCode.Key_I,
                Key.Insert => KeyCode.Key_INSERT,
                Key.J => KeyCode.Key_J,
                Key.K => KeyCode.Key_K,
                Key.L => KeyCode.Key_L,
                Key.LeftAlt => KeyCode.Key_LMENU,
                Key.Left => KeyCode.Key_LEFT,
                Key.LeftBracket => KeyCode.Key_LBRACKET,
                Key.LeftControl => KeyCode.Key_LCONTROL,
                Key.LeftShift => KeyCode.Key_LSHIFT,
                Key.LeftWindowsKey => KeyCode.Key_LWIN,
                Key.M => KeyCode.Key_M,
                Key.Mail => KeyCode.Key_MAIL,
                Key.MediaSelect => KeyCode.Key_MEDIASELECT,
                Key.MediaStop => KeyCode.Key_MEDIASTOP,
                Key.Minus => KeyCode.Key_MINUS,
                Key.Mute => KeyCode.Key_MUTE,
                Key.MyComputer => KeyCode.Key_MYCOMPUTER,
                Key.N => KeyCode.Key_N,
                Key.O => KeyCode.Key_O,
                Key.Oem102 => KeyCode.Key_OEM_102,
                Key.P => KeyCode.Key_P,
                Key.PageDown => KeyCode.Key_PGDOWN,
                Key.PageUp => KeyCode.Key_PGUP,
                Key.Pause => KeyCode.Key_PAUSE,
                Key.Period => KeyCode.Key_PERIOD,
                Key.PlayPause => KeyCode.Key_PLAYPAUSE,
                Key.Power => KeyCode.Key_POWER,
                Key.PreviousTrack => KeyCode.Key_PREVTRACK,
                Key.PrintScreen => KeyCode.Key_SYSRQ,
                Key.Q => KeyCode.Key_Q,
                Key.R => KeyCode.Key_R,
                Key.Return => KeyCode.Key_RETURN,
                Key.RightAlt => KeyCode.Key_RMENU,
                Key.Right => KeyCode.Key_RIGHT,
                Key.RightBracket => KeyCode.Key_RBRACKET,
                Key.RightControl => KeyCode.Key_RCONTROL,
                Key.RightShift => KeyCode.Key_RSHIFT,
                Key.RightWindowsKey => KeyCode.Key_RWIN,
                Key.S => KeyCode.Key_S,
                Key.ScrollLock => KeyCode.Key_SCROLL,
                Key.Semicolon => KeyCode.Key_SEMICOLON,
                Key.Slash => KeyCode.Key_SLASH,
                Key.Sleep => KeyCode.Key_SLEEP,
                Key.Space => KeyCode.Key_SPACE,
                Key.Stop => KeyCode.Key_STOP,
                Key.T => KeyCode.Key_T,
                Key.Tab => KeyCode.Key_TAB,
                Key.U => KeyCode.Key_U,
                Key.Underline => KeyCode.Key_UNDERLINE,
                Key.Unknown => KeyCode.Key_UNASSIGNED,
                Key.Unlabeled => KeyCode.Key_UNLABELED,
                Key.Up => KeyCode.Key_UP,
                Key.V => KeyCode.Key_V,
                Key.VolumeDown => KeyCode.Key_VOLUMEDOWN,
                Key.VolumeUp => KeyCode.Key_VOLUMEUP,
                Key.W => KeyCode.Key_W,
                Key.Wake => KeyCode.Key_WAKE,
                Key.WebBack => KeyCode.Key_WEBBACK,
                Key.WebFavorites => KeyCode.Key_WEBFAVORITES,
                Key.WebForward => KeyCode.Key_WEBFORWARD,
                Key.WebHome => KeyCode.Key_WEBHOME,
                Key.WebRefresh => KeyCode.Key_WEBREFRESH,
                Key.WebSearch => KeyCode.Key_WEBSEARCH,
                Key.WebStop => KeyCode.Key_WEBSTOP,
                Key.X => KeyCode.Key_X,
                Key.Y => KeyCode.Key_Y,
                Key.Yen => KeyCode.Key_YEN,
                Key.Z => KeyCode.Key_Z,
                Key.Multiply => KeyCode.Key_MULTIPLY,
                Key.NumberLock => KeyCode.Key_NUMLOCK,
                Key.NumberPad7 => KeyCode.Key_NUMPAD7,
                Key.NumberPad8 => KeyCode.Key_NUMPAD8,
                Key.NumberPad9 => KeyCode.Key_NUMPAD9,
                Key.Subtract => KeyCode.Key_SUBTRACT,
                Key.NumberPad4 => KeyCode.Key_NUMPAD4,
                Key.NumberPad5 => KeyCode.Key_NUMPAD5,
                Key.NumberPad6 => KeyCode.Key_NUMPAD6,
                Key.Add => KeyCode.Key_ADD,
                Key.NumberPad1 => KeyCode.Key_NUMPAD1,
                Key.NumberPad2 => KeyCode.Key_NUMPAD2,
                Key.NumberPad3 => KeyCode.Key_NUMPAD3,
                Key.NumberPad0 => KeyCode.Key_NUMPAD0,
                Key.Decimal => KeyCode.Key_DECIMAL,
                Key.Kana => KeyCode.Key_KANA,
                Key.NoConvert => KeyCode.Key_NOCONVERT,
                Key.NumberPadEquals => KeyCode.Key_NUMPADEQUALS,
                Key.Kanji => KeyCode.Key_KANJI,
                Key.NextTrack => KeyCode.Key_NEXTTRACK,
                Key.NumberPadEnter => KeyCode.Key_NUMPADENTER,
                Key.NumberPadComma => KeyCode.Key_NUMPADCOMMA,
                Key.Divide => KeyCode.Key_DIVIDE,
                _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            };
        }

        #endregion Methods

        #region InputObject Implementation

        public override void Capture()
        {
            if (IsBuffered)
                ReadBuffered();
            else
                Read();
        }

        protected override void Initialize()
        {
            this._keyboard = new SharpDX.DirectInput.Keyboard(this._directInput);

            //_keyboard.SetDataFormat( MDI.DeviceDataFormat.Keyboard );

            if (this._coopSettings != 0)
                this._keyboard.SetCooperativeLevel(((DirectXInputManager) Creator).WindowHandle, this._coopSettings);

            if (IsBuffered)
                this._keyboard.Properties.BufferSize = BufferSize;

            try
            {
                this._keyboard.Acquire();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to aquire keyboard using DirectInput.", e);
            }
        }

        #endregion InputObject Implementation

        #region Keyboard Implementation

        public override int[] KeyStates
        {
            get { return (int[]) this._keyboardState.Clone(); }
        }

        public override bool IsKeyDown(KeyCode key)
        {
            return ((this._keyboardState[(int) key]) != 0);
        }

        public override string AsString(KeyCode key)
        {
            return this._keyboard.Properties.GetKeyName((Key) key);
        }

        #endregion Keyboard Implementation
    }
}
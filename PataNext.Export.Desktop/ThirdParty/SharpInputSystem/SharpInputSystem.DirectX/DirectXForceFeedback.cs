#region MIT/X11 License

/*
Sharp Input System Library
Copyright © 2007-2011 Michael Cummings

The overall design, and a majority of the core code contained within 
this library is a derivative of the open source Open Input System ( OIS ) , 
which can be found at http://www.sourceforge.net/projects/wgois.  
Many thanks to the Phillip Castaneda for maintaining such a high quality project.

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

using MDI = SharpDX.DirectInput;

#endregion Namespace Declarations

namespace SharpInputSystem.DirectX
{
    internal class DirectXForceFeedback : ForceFeedback
    {
        #region Construction and Destruction

        public DirectXForceFeedback(MDI.Joystick parent, MDI.Capabilities capabilities) { }

        #endregion

        #region ForceFeedback Implementation

        #region Properties

        public override float MasterGain
        {
            set { }
        }

        public override bool AutoCenterMode
        {
            set { }
        }

        public override int SupportedAxesCount
        {
            get { return 0; }
        }

        #endregion Properties

        #region Methods

        public override void Upload(Effect effect) { }

        public override void Modify(Effect effect) { }

        public override void Remove(Effect effect) { }

        #endregion Methods

        #endregion ForceFeedback Implementation
    }
}

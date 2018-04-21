using UnityEngine.Experimental.Input.Utilities;

namespace Packages.pack.guerro.shared.Scripts.Utilities
{
    public static class InputDimension
    {
        private static string nX = "-x";
        private static string pX = "+x";
        private static string nY = "-y";
        private static string pY = "+y";
        
        public static InternedString GetDimensionStringId(int index)
        {
            if (index == 0) return new InternedString(nX);
            if (index == 1) return new InternedString(pX);
            if (index == 2) return new InternedString(nY);
            if (index == 3) return new InternedString(pY);
            return new InternedString("?d");
        }
    }
}
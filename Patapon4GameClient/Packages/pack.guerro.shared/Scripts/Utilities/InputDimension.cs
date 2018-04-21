using UnityEngine.Experimental.Input.Utilities;

namespace Packages.pack.guerro.shared.Scripts.Utilities
{
    public static class InputDimension
    {
        public static InternedString GetDimensionStringId(int index)
        {
            if (index == 0) return new InternedString("-x");
            if (index == 1) return new InternedString("+x");
            if (index == 2) return new InternedString("-y");
            if (index == 3) return new InternedString("+y");
            return new InternedString("?d");
        }
    }
}
using UnityEngine;

namespace Packages.pack.guerro.shared.Scripts.Utilities
{
    public static class BoundsExtensions
    {
        public static bool Contains(this Bounds b1, Bounds b2)
        {
            Debug.Log($"{b1.min}, {b1.max}; {b2.min}, {b2.max}");
            
            return b1.Contains(b2.min) && b1.Contains(b2.max);
        }

        public static void ApplyFlat2D(this Bounds b1)
        {
            var center = b1.center;
            var extents = b1.extents;

            center.z = 0;
            extents.z = 0;
            
            b1.center = center;
            b1.extents = extents;
        }
        
        public static Bounds Flat2D(this Bounds b1)
        {
            var center  = b1.center;
            var extents = b1.extents;

            center.z  = 0;
            extents.z = 0;
            
            b1.center  = center;
            b1.extents = extents;

            return b1;
        }
    }
}
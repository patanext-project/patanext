using Unity.Entities;
using UnityEngine;

namespace P4.Default.Movements
{
    public struct P4Default_DFreeMovementData : IComponentData
    {
        /// <summary>
        /// Speed exprimed km/h
        /// </summary>
        public float BaseSpeed;
        /// <summary>
        /// Factor exprimed in km/h (BaseSpeed * AerialMovementReductionFactor).
        /// Should be less than 1 and not negative.
        /// </summary>
        public float AerialMovementReductionFactor;
    }

    [AddComponentMenu("Moddable/P4Default/Free Movement")]
    public class P4Default_DFreeMovementWrapper : ComponentDataWrapper<P4Default_DFreeMovementData>
    {
    }
}
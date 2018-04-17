using System;
using Unity.Entities;
using Unity.Mathematics;

namespace P4.Default
{
    [Serializable]
    public struct P4Default_DEntityFreeInputData : IComponentData
    {
        /// <summary>
        /// The direction of the entity
        /// </summary>
        public float2 MoveDirection;
    }
    
    public class P4Default_DEntityFreeInputWrapper : ComponentDataWrapper<P4Default_DEntityFreeInputData>
    {
        
    }
}
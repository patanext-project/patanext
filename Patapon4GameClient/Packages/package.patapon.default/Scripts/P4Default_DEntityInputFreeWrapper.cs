using System;
using Unity.Entities;
using Unity.Mathematics;

namespace P4.Default
{
    public struct P4Default_DEntityInputFreeData : IComponentData
    {
        /// <summary>
        /// The direction of the entity
        /// </summary>
        public float2 MoveDirection;
    }

    public class P4Default_DEntityInputFreeWrapper : ComponentDataWrapper<P4Default_DEntityInputFreeData>
    {
        
    }
}
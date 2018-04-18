using System;
using Unity.Entities;
using Unity.Mathematics;

namespace P4Main.Components
{
    [Serializable]
    public struct FreeMovementInputReceiver : IComponentData
    {
        public int MovementDirection;
        public bool1 IsSprinting;
    }

    public class FreeMovementInputReceiverComponent : ComponentDataWrapper<FreeMovementInputReceiver>
    {
        
    }
}
using Unity.Entities;
using Unity.Mathematics;

namespace Packet.Guerro.Shared.Game
{
    public struct ControllableEntity : ISharedComponentData
    {
        public bool1 IsCreated;
        public EEntityControl ControlType;
        public EntityGroup    GroupId;
    }

    public enum EEntityControl
    {
        None     = 0,
        Always   = 1,
        ByGroup  = 2,
        Scripted = 3
    }

    public class ControllableEntityWrapper : SharedComponentDataWrapper<ControllableEntity>
    {
        
    }
}
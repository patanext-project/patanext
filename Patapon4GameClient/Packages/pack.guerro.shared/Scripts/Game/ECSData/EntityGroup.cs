using Unity.Entities;
using Unity.Mathematics;

namespace Packet.Guerro.Shared.Game
{
    public struct EntityGroup : IComponentData
    {
        public bool1 IsCreated;
        public int ReferenceId;
        public int Version;
    }
}
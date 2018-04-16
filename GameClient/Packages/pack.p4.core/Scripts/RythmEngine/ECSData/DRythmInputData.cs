using Packet.Guerro.Shared.Game;
using Unity.Entities;

namespace P4.Core.RythmEngine
{
    public struct DRythmInputData : IComponentData
    {
        public int Key;
        public EntityGroup EntityGroup;
    }
}
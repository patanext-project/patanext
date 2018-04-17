using Unity.Entities;
using Unity.Mathematics;
using System;

namespace Packet.Guerro.Shared.Network
{
    [Serializable]
    public struct NetworkEntity : ISharedComponentData
    {
        /// <summary>
        /// Is the component created?
        /// </summary>
        public bool1 IsCreated;
        /// <summary>
        /// Is the entity local?
        /// </summary>
        public bool1 IsLocal;
        /// <summary>
        /// Id of the entity controlled by a local player (Split screen)
        /// </summary>
        public int LocalControlId;
        /// <summary>
        /// Id of the entity controlled by a networked player
        /// </summary>
        public int NetworkControlId;
    }

    public class NetworkEntityWrapper : SharedComponentDataWrapper<NetworkEntity>
    {

    }
}
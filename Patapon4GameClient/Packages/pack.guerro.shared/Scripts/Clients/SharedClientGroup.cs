using Packet.Guerro.Shared.Clients;
using Unity.Entities;

namespace Packages.pack.guerro.shared.Scripts.Clients
{
    public struct SharedClientGroup
    {
        /// <summary>
        /// The clients
        /// </summary>
        public ComponentDataArray<ClientEntity> Clients;

        /// <summary>
        /// The length of the group
        /// </summary>
        public int Length;
    }
}
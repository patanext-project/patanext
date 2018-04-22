using Unity.Entities;

namespace Packet.Guerro.Shared.Clients
{
    public struct ClientEntityAttach : IComponentData
    {
        public ClientEntity AttachedTo;
    }
}
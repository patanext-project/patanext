using Guerro.Utilities;
using Unity.Entities;
using UnityEngine;

namespace Packet.Guerro.Shared.Network.Entities
{
    public class CNetworkEntityManager : ComponentSystem
    {
        protected override void OnUpdate()
        {

        }

        public NetworkEntity AddOrSetComponent(Entity entity, NetworkEntity data = default(NetworkEntity))
        {
            data.IsCreated = true;
            entity.SetOrCreateSharedComponentData(data);

            return data;
        }

        public NetworkEntity AddOrSetComponent(Entity entity, GameObject optionalGameObject, NetworkEntity data = default(NetworkEntity))
        {
            data = AddOrSetComponent(entity, data);
            var wrapper = optionalGameObject.GetComponent<NetworkEntityWrapper>()
                          ?? optionalGameObject.AddComponent<NetworkEntityWrapper>();
            wrapper.Value = data;
            return data;
        }
    }
}
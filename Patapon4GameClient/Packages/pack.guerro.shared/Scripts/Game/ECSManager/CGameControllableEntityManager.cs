using Guerro.Utilities;
using Packet.Guerro.Shared.Network;
using Unity.Entities;
using UnityEngine;

namespace Packet.Guerro.Shared.Game
{
    public class CGameControllableEntityManager : ComponentSystem
    {
        protected override void OnUpdate()
        {

        }

        public ControllableEntity AddOrSetComponent(Entity entity, ControllableEntity data = default(ControllableEntity))
        {
            data.IsCreated = true;
            entity.SetOrCreateSharedComponentData(data);

            return data;
        }

        public ControllableEntity AddOrSetComponent(Entity entity, GameObject optionalGameObject, ControllableEntity data = default(ControllableEntity))
        {
            data = AddOrSetComponent(entity);
            var wrapper = optionalGameObject.GetComponent<ControllableEntityWrapper>()
                          ?? optionalGameObject.AddComponent<ControllableEntityWrapper>();
            wrapper.Value = data;
            return data;
        }
    }
}
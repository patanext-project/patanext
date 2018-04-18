using UnityEngine;
using Unity.Entities;
using P4Main.Components;
using Packet.Guerro.Shared.Network;

namespace P4Main.Systems
{
    public class DataInputFreeMoveProcessorSystem : ComponentSystem
    {
        ComponentGroup m_Group;

        protected override void OnCreateManager(int capacity)
        {
            m_Group = GetComponentGroup(typeof(FreeMovementInputReceiver), typeof(NetworkEntity));
        }

        protected override void OnUpdate()
        {
            m_Group.SetFilter(new NetworkEntity() { IsLocal = true });
            var networkComponents   = m_Group.GetSharedComponentDataArray<NetworkEntity>();
            var inputComponents     = m_Group.GetComponentDataArray<FreeMovementInputReceiver>();

            for (int i = 0; i != networkComponents.Length; i++)
            {
                var netComponent    = networkComponents[i];
                var inputComponent  = inputComponents[i];

                if (netComponent.LocalControlId == 0) //< We only work with the first player for now
                {

                }
            }
        }
    }
}
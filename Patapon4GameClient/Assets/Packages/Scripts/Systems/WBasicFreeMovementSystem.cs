using UnityEngine;
using Unity.Entities;
using P4Main.Components;

namespace P4Main.Systems
{
    public class WBasicFreeMovementSystem : ComponentSystem
    {
        struct ComponentGroup
        {
            public ComponentDataArray<DefaultFreeMovement> Components;
            public ComponentDataArray<FreeMovementInputReceiver> Inputs;
            public int Length;
        }

        private ComponentGroup m_Group;

        protected override void OnCreateManager(int capacity)
        {
            
        }

        protected override void OnUpdate()
        {
            for (int i = 0; i < m_Group.Length; i++)
            {
                var component = m_Group.Components[i];
                var input = m_Group.Inputs[i];
            }
        }

    }
}
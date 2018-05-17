using JetBrains.Annotations;
using P4.Core.RythmEngine;
using Packet.Guerro.Shared.Game;
using Unity.Entities;
using UnityEngine;

namespace P4.Core.RythmEngine
{
    [UsedImplicitly]
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(RythmBeatSystem))]
    public class RythmSystem : ComponentSystem
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Groups
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public struct GroupCommands
        {
            public EntityArray Entities;
            public ComponentDataArray<DRythmInputData> Inputs;
            public int Length;
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        [Inject] private EndFrameBarrier m_Barrier;
        [Inject] private GroupCommands m_GroupCommands;
        
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Base Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        protected override void OnUpdate()
        {
            for (int index = 0; index != m_GroupCommands.Length; ++index)
            {
                var entity = m_GroupCommands.Entities[index];
                var input = m_GroupCommands.Inputs[index];
                
                Debug.Log($"New rythm input! {input.Key} from group {input.EntityGroup.ReferenceId}");
                
                EntityManager.DestroyEntity(entity);
            }
        }

        internal void AddBeatEvent()
        {
            
        }

        public void AddClientInputEvent(int key, EntityGroup entityGroup)
        {
            var archetype = EntityManager.CreateArchetype(typeof(DRythmInputData));
            
            var entity = EntityManager.CreateEntity(archetype);
            EntityManager.SetComponentData(entity, new DRythmInputData()
            {
                Key = key,
                EntityGroup = entityGroup
            });
        }
    }
}
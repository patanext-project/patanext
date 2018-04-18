using JetBrains.Annotations;
using P4.Core.RythmEngine;
using Unity.Entities;

namespace P4.Core.RythmEngine
{
    [UsedImplicitly]
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
                
                m_Barrier.PostUpdateCommands.DestroyEntity(entity);
            }
        }
    }
}
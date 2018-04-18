using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace P4.Default.Inputs
{
    public class P4Default_EntityInputFreeSystem : ComponentSystem
    {
        struct InputGroup
        {
            public ComponentDataArray<P4Default_DEntityInputFreeData> InputData;
            public ComponentDataArray<P4Default_DEntityInputUnifiedData> UnifiedInputs;
            
            public int Length;
        }

        [Inject] private InputGroup m_Group;
        
        protected override void OnUpdate()
        {
            for (int i = 0; i != m_Group.Length; i++)
            {
                var input = m_Group.InputData[i];           
                input.MoveDirection = new float2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

                var unifiedInput = m_Group.UnifiedInputs[i];
                unifiedInput.FreeInput = input;

                m_Group.InputData[i] = input;
                m_Group.UnifiedInputs[i] = unifiedInput;
            }
        }
    }
}
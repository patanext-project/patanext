using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace P4.Default.Inputs
{
    public class P4Default_CEntityInputFree
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
                
                //input.MoveDirection = new float2(CInput., 0);

                var unifiedInput = m_Group.UnifiedInputs[i];
                unifiedInput.FreeInput = input;

                m_Group.InputData[i] = input;
                m_Group.UnifiedInputs[i] = unifiedInput;
            }
        }
    }
}
using System.Globalization;
using Packages.pack.guerro.shared.Scripts.Modding;
using Packet.Guerro.Shared.Inputs;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Input;

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
        
        protected override void OnCreateManager(int capacity)
        {
            var modWorld = CModInfo.CurrentModWorld;
            var modInputManager = modWorld.GetOrCreateManager<ModInputManager>();
        }

        protected override void OnUpdate()
        {       
            for (int i = 0; i != m_Group.Length; i++)
            {
                var input = m_Group.InputData[i];

                var unifiedInput = m_Group.UnifiedInputs[i];
                unifiedInput.FreeInput = input;

                m_Group.InputData[i] = input;
                m_Group.UnifiedInputs[i] = unifiedInput;
            }
        }
    }
}
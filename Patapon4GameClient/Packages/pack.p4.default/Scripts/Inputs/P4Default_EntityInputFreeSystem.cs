using System.Globalization;
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

        private InputAction m_LeftArrow, m_RightArrow, m_DpadLeft, m_DpadRight;
        
        protected override void OnCreateManager(int capacity)
        {
            m_LeftArrow = new InputAction(binding: "keyboard/leftArrow");
            m_RightArrow = new InputAction(binding: "keyboard/rightArrow");
            m_DpadLeft = new InputAction(binding: "<gamepad>/dpad/left");
            m_DpadRight = new InputAction(binding: "<gamepad>/dpad/right");

            m_LeftArrow.performed += _ => Debug.Log("m_leftArrow");
            m_RightArrow.performed += _ => Debug.Log("m_RightArrow");
            m_DpadLeft.performed += _ => Debug.Log("m_DpadLeft");
            m_DpadRight.performed += _ => Debug.Log("m_DpadRight");
            
            m_LeftArrow.Enable();
            m_RightArrow.Enable();
            m_DpadLeft.Enable();
            m_DpadRight.Enable();
        }

        protected override void OnUpdate()
        {       
            for (int i = 0; i != m_Group.Length; i++)
            {
                var input = m_Group.InputData[i];

                Debug.Log(Gamepad.current["dpad/left"].ReadValueAsObject().ToString());

                var unifiedInput = m_Group.UnifiedInputs[i];
                unifiedInput.FreeInput = input;

                m_Group.InputData[i] = input;
                m_Group.UnifiedInputs[i] = unifiedInput;
            }
        }
    }
}
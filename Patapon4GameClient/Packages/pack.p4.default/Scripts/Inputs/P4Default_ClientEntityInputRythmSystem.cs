using System;
using System.Collections.Generic;
using Packages.pack.guerro.shared.Scripts.Clients;
using Packages.pack.guerro.shared.Scripts.Modding;
using Packet.Guerro.Shared.Clients;
using Packet.Guerro.Shared.Inputs;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace P4.Default.Inputs
{
    public class P4Default_ClientEntityInputRythmSystem : ComponentSystem
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Sub-classes
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public class ClientContainer : ClientDataContainer
        {
            public int[] InputIds;
            public InputAction[] InputActions;

            protected override void OnCreateContainer()
            {
                InputIds = new int[4];
                InputActions = new InputAction[4];
            }

            protected override void OnDestroyContainer()
            {
                if (InputActions != null)
                {
                    for (int i = 0; i != InputActions.Length; i++)
                        InputActions[i].Disable();
                }

                InputIds = null;
                InputActions = null;
            }
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Groups
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public struct ClientGroup
        {
            public ComponentDataArray<ClientEntityAttach>              AttachData;
            public ComponentDataArray<P4Default_DEntityInputRythmData> InputData;

            public int Length;
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        [Inject] private ClientGroup   m_ClientGroup;
        [Inject] private CInputManager m_InputManager;

        [CModInfo.Inject] private CModInfo m_ModInfo;

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Base methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        protected override void OnCreateManager(int capacity)
        {
            ClientManager.OnNewClient             += OnNewClient;
            CInputManager.OnClientDeviceChanged   += InputOnClientUpdate;
            CInputManager.OnClientSettingsChanged += InputOnClientUpdate;
            
            m_ModInfo = CModInfo.CurrentMod;

            var modInputManager = m_ModInfo.GetInputManager();
            modInputManager.RegisterFromFile("rythm_inputs");
        }

        protected override void OnUpdate()
        {
            for (int i = 0; i != m_ClientGroup.Length; i++)
            {
                var client = m_ClientGroup.AttachData[i].AttachedTo;
                if (!client.IsAlive())
                    continue;

                var inputManager = client.GetWorld().GetOrCreateManager<ClientInputManager>();
            }
        }

        protected override void OnDestroyManager()
        {
            base.OnDestroyManager();
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Methods from events
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        private void InputOnClientUpdate(ClientInputManager clientInputManager)
        {
            var container   = clientInputManager.ClientWorld.GetExistingContainer<ClientContainer>();
            //var inputAction = clientInputManager.GetInputAction();
        }

        private void OnNewClient(ClientEntity clientId)
        {
            var inputData = clientId.GetWorld().SetContainer<ClientContainer>(new ClientContainer());
            for (int i = 0; i != 4; i++)
            {
                inputData.InputIds[i] = m_ModInfo.GetInputManager().GetId("action" + i);
            }

            var clientInputManager = clientId.GetInputManager();
            clientInputManager.CreateInputAction(inputData.InputIds[0]).Performed += context => Debug.Log("On perform");
        }
    }
}
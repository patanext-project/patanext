using System;
using System.Collections.Generic;
using P4.Core.RythmEngine;
using Packages.pack.guerro.shared.Scripts.Clients;
using Packages.pack.guerro.shared.Scripts.Modding;
using Packet.Guerro.Shared.Clients;
using Packet.Guerro.Shared.Inputs;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Experimental.Input.Utilities;

namespace P4.Default.Inputs
{
    public class P4Default_ClientEntityInputRythmSystem : ComponentSystem
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Sub-classes
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public class ClientContainer : ClientDataContainer
        {

            protected override void OnCreateContainer()
            {
            }

            protected override void OnDestroyContainer()
            {
            }
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Groups
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public struct Group
        {
            public ComponentDataArray<ClientEntityAttach>              AttachData;
            public ComponentDataArray<P4Default_DEntityInputRythmData> InputData;

            public int Length;
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public ReadOnlyArray<int> InputIds => new ReadOnlyArray<int>(m_actionInputIds);

        private int[] m_actionInputIds;

        [Inject] private Group         m_Group;
        [Inject] private CInputManager m_InputManager;
        [Inject] private RythmSystem m_RythmSystem;

        [CModInfo.Inject] private CModInfo m_ModInfo;

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Base methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        protected override void OnCreateManager(int capacity)
        {
            ClientManager.OnNewClient += OnNewClient;

            m_ModInfo = CModInfo.CurrentMod;

            var modInputManager = m_ModInfo.GetInputManager();
            modInputManager.RegisterFromFile("rythm_inputs");

            m_actionInputIds = new int[4];
            for (int i = 0; i != 4; i++)
            {
                m_actionInputIds[i] = modInputManager.GetId("action" + (i + 1));
            }
        }

        protected override void OnUpdate()
        {
            for (int i = 0; i != m_Group.Length; i++)
            {
                var client = m_Group.AttachData[i].AttachedTo;
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
        private void OnInputStarted(InputAction.CallbackContext ctx, ClientEntity client, int inputId)
        {
            var index = Array.IndexOf(m_actionInputIds, inputId);

            for (int i = 0; i != m_Group.Length; i++)
            {
                var clientAttach = m_Group.AttachData[i];
                if (!clientAttach.AttachedTo.Equals(client))
                    continue;

                var inputData = m_Group.InputData[i];

                if (index == 0) inputData.KeyAction1 = P4Default_EEntityInputRythmActionState.Pressed;
                if (index == 1) inputData.KeyAction2 = P4Default_EEntityInputRythmActionState.Pressed;
                if (index == 2) inputData.KeyAction3 = P4Default_EEntityInputRythmActionState.Pressed;
                if (index == 3) inputData.KeyAction4 = P4Default_EEntityInputRythmActionState.Pressed;

                m_Group.InputData[i] = inputData;
            }
 
            m_RythmSystem.AddClientInputEvent(index, client.GetGroup());
        }

        private void OnInputEnded(InputAction.CallbackContext ctx, ClientEntity client, int inputId)
        {
            var index = Array.IndexOf(m_actionInputIds, inputId);

            for (int i = 0; i != m_Group.Length; i++)
            {
                var clientAttach = m_Group.AttachData[i];
                if (!clientAttach.AttachedTo.Equals(client))
                    continue;

                var inputData = m_Group.InputData[i];

                if (index == 0) inputData.KeyAction1 = P4Default_EEntityInputRythmActionState.None;
                if (index == 1) inputData.KeyAction2 = P4Default_EEntityInputRythmActionState.None;
                if (index == 2) inputData.KeyAction3 = P4Default_EEntityInputRythmActionState.None;
                if (index == 3) inputData.KeyAction4 = P4Default_EEntityInputRythmActionState.None;

                m_Group.InputData[i] = inputData;
            }
        }

        private void OnNewClient(ClientEntity clientId)
        {
            var inputManager = clientId.GetInputManager();
            var inputData    = clientId.GetWorld().SetContainer(new ClientContainer());
            for (int i = 0; i != 4; i++)
            {
                var id          = m_actionInputIds[i];
                var inputAction = inputManager.CreateInputAction(id);

                inputAction.HackyStartCancel =  true;
                inputAction.Started          += OnInputStarted;
                inputAction.Cancelled        += OnInputEnded;
            }
        }
    }
}
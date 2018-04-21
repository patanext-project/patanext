using System.Collections.Generic;
using Packet.Guerro.Shared.Clients;
using Packet.Guerro.Shared.Inputs;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.Input;

// ReSharper disable HeapView.BoxingAllocation

namespace Assets.Units.InputSystemTest
{
    public class Unit_InputSystemTest_Bootstrap : MonoBehaviour
    {
        public ClientEntity ClientId;

        public int InputJumpId;
        public int InputHorizontalId;

        private void Start()
        {
            var inputManager = World.Active.GetExistingManager<CInputManager>();

            var inputList = new List<CInputManager.IInputSetting>
            {
                new CInputManager.Settings.Push
                (
                    nameId: "jump",
                    displayName: "Jump",
                    translation: "%m.inputs.jump",
                    defaults: new FastDictionary<string, string[]>
                    {
                        {
                            "<keyboard>",
                            new[] {"space"}
                        },
                        {
                            "<gamepad>",
                            new[] {"buttonSouth"}
                        }
                    }
                ),
                new CInputManager.Settings.Axis1D
                (
                    nameId: "horizontal",
                    displayName: "Horizontal",
                    translation: "%m.inputs.horizontal",
                    defaults: new FastDictionary<string, FastDictionary<string, string[]>>
                    {
                        {
                            "<keyboard>",
                            new FastDictionary<string, string[]>
                            {
                                {"-x", new[] {"leftArrow"}},
                                {"+x", new[] {"rightArrow"}}
                            }
                        },
                        {
                            "<gamepad>",
                            new FastDictionary<string, string[]>
                            {
                                {"-x", new[] {"dpad/left"}},
                                {"+x", new[] {"dpad/right"}}
                            }
                        }
                    }
                )
            };

            inputManager.RegisterFromList(inputList);

            // Get our ids
            InputJumpId       = inputManager.GetId("jump");
            InputHorizontalId = inputManager.GetId("horizontal");

            // Create a client
            var clientManager = World.Active.GetExistingManager<ClientManager>();
            ClientId = clientManager.Create("debug");
        }

        private void Update()
        {
            var clientManager = World.Active.GetExistingManager<ClientManager>();
            var clientWorld   = clientManager.GetWorld(ClientId);
            var inputManager  = clientWorld.GetOrCreateManager<ClientInputManager>();
            
            inputManager.ActiveDevice = inputManager.ActiveDevice ?? Keyboard.current;
            if (Keyboard.current.anyKey.isPressed)
                inputManager.ActiveDevice = Keyboard.current;
            if (Gamepad.current.buttonSouth.isPressed)
                inputManager.ActiveDevice = Gamepad.current;

            Debug.Log(inputManager.Get<CInputManager.Result.Axis1D>(InputHorizontalId).Value);
        }

        private void OnGUI()
        {
            var clientManager = World.Active.GetExistingManager<ClientManager>();
            var clientWorld   = clientManager.GetWorld(ClientId);
            var inputManager  = clientWorld.GetOrCreateManager<ClientInputManager>();
            
            GUI.Label(new Rect(5, 5, 200, 200), $"Device: {inputManager.ActiveDevice.displayName}");
        }
    }
}
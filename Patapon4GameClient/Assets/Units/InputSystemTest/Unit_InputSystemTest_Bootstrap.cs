using System.Collections.Generic;
using Packet.Guerro.Shared.Clients;
using Packet.Guerro.Shared.Inputs;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Profiling;

// ReSharper disable HeapView.BoxingAllocation

namespace Assets.Units.InputSystemTest
{
    public class Unit_InputSystemTest_Bootstrap : MonoBehaviour
    {
        public ClientEntity ClientId;

        public int InputJumpId;
        public int InputHorizontalId;
        public int InputHorizontalAndVerticalId;

        public int Action1Id;
        public int Action2Id;
        public int Action3Id;
        public int Action4Id;

        public bool    IsJumping;
        public float   HorizontalValue;
        public Vector2 HorizontalAndVerticalValue;

        public bool IsRythm1;
        public bool IsRythm2;
        public bool IsRythm3;
        public bool IsRythm4;

        private void Start()
        {
            var inputManager = World.Active.GetExistingManager<CInputManager>();

            var inputList = new List<CInputManager.InputSettingBase>
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
                new CInputManager.Settings.Push
                (
                    nameId: "action1",
                    displayName: "Jump",
                    translation: "%m.inputs.jump",
                    defaults: new FastDictionary<string, string[]>
                    {
                        {
                            "<keyboard>",
                            new[] {"numpad4"}
                        },
                        {
                            "<gamepad>",
                            new[] {"buttonWest"}
                        }
                    }
                ),
                new CInputManager.Settings.Push
                (
                    nameId: "action2",
                    displayName: "Jump",
                    translation: "%m.inputs.jump",
                    defaults: new FastDictionary<string, string[]>
                    {
                        {
                            "<keyboard>",
                            new[] {"numpad6"}
                        },
                        {
                            "<gamepad>",
                            new[] {"buttonEast"}
                        }
                    }
                ),
                new CInputManager.Settings.Push
                (
                    nameId: "action3",
                    displayName: "Jump",
                    translation: "%m.inputs.jump",
                    defaults: new FastDictionary<string, string[]>
                    {
                        {
                            "<keyboard>",
                            new[] {"numpad2"}
                        },
                        {
                            "<gamepad>",
                            new[] {"buttonSouth"}
                        }
                    }
                ),
                new CInputManager.Settings.Push
                (
                    nameId: "action4",
                    displayName: "Jump",
                    translation: "%m.inputs.jump",
                    defaults: new FastDictionary<string, string[]>
                    {
                        {
                            "<keyboard>",
                            new[] {"numpad8"}
                        },
                        {
                            "<gamepad>",
                            new[] {"buttonNorth"}
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
                ),
                new CInputManager.Settings.Axis2D
                (
                    nameId: "horizontal&vertical",
                    displayName: "Horizontal And Vertical",
                    translation: "%m.inputs.horizontal&vertical",
                    defaults: new FastDictionary<string, FastDictionary<string, string[]>>
                    {
                        {
                            "<keyboard>",
                            new FastDictionary<string, string[]>
                            {
                                {"-x", new[] {"leftArrow"}},
                                {"+x", new[] {"rightArrow"}},
                                {"-y", new[] {"downArrow"}},
                                {"+y", new[] {"upArrow"}}
                            }
                        },
                        {
                            "<gamepad>",
                            new FastDictionary<string, string[]>
                            {
                                {"-x", new[] {"dpad/left"}},
                                {"+x", new[] {"dpad/right"}},
                                {"-y", new[] {"dpad/down"}},
                                {"+y", new[] {"dpad/up"}}
                            }
                        }
                    }
                )
            };

            inputManager.RegisterFromList(inputList);

            // Get our ids
            InputJumpId                  = inputManager.GetId("jump");
            InputHorizontalId            = inputManager.GetId("horizontal");
            InputHorizontalAndVerticalId = inputManager.GetId("horizontal&vertical");

            Action1Id = inputManager.GetId("action1");
            Action2Id = inputManager.GetId("action2");
            Action3Id = inputManager.GetId("action3");
            Action4Id = inputManager.GetId("action4");

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

            // There will be multiple inputs, so let's try to stress test a bit
            Profiler.BeginSample("Forloop");
            for (int i = 0; i != 25; i++)
            {
                IsJumping                  = inputManager.GetPush(InputJumpId).Value > 0.5f;
                HorizontalValue            = inputManager.GetAxis1D(InputHorizontalId).Value;
                HorizontalAndVerticalValue = inputManager.GetAxis2D(InputHorizontalAndVerticalId).Value;

                IsRythm1 = inputManager.GetPush(Action1Id).Value > 0.5f;
                IsRythm2 = inputManager.GetPush(Action2Id).Value > 0.5f;
                IsRythm3 = inputManager.GetPush(Action3Id).Value > 0.5f;
                IsRythm4 = inputManager.GetPush(Action4Id).Value > 0.5f; 
            }
            Profiler.EndSample();
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
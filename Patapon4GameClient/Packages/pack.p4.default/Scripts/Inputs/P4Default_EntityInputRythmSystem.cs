using System.Collections.Generic;
using Packages.pack.guerro.shared.Scripts.Modding;
using Packet.Guerro.Shared.Clients;
using Packet.Guerro.Shared.Inputs;
using Unity.Entities;

namespace P4.Default.Inputs
{
    public class P4Default_EntityInputRythmSystem : ComponentSystem
    {
        protected override void OnCreateManager(int capacity)
        {
            var inputManager = CModInfo.CurrentModWorld
                .GetOrCreateManager<ModInputManager>();
            
            var inputList = new List<CInputManager.InputSettingBase>
            {
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
                )
            };

            //inputManager.RegisterFromList(inputList);
            inputManager.RegisterFromFile("rythm_inputs");

            ClientManager.OnNewClient += OnNewClient;
        }

        protected override void OnUpdate()
        {
            
        }

        protected override void OnDestroyManager()
        {
            ClientManager.OnNewClient -= OnNewClient;
        }

        protected void OnNewClient(ClientEntity clientId)
        {
            var clientWorld = clientId.GetWorld();
            //var clientInputManager = clientWorld.GetExistingManager<P4Default_Rythm>();
        }
    }
}
using System;
using System.Collections.Generic;
using Packages.pack.guerro.shared.Scripts.Clients;
using Packet.Guerro.Shared.Clients;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.iOS;

namespace Packet.Guerro.Shared.Inputs
{    
    public partial class CInputManager : ComponentSystem
    {
        
        public static event Action<ClientInputManager> OnClientDeviceUpdate;
        public static event Action<ClientInputManager> OnClientSettingsChanged;
        public static event Action<ClientInputManager> OnClientDeviceChanged;
        
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Group
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        [Inject] private SharedClientGroup m_ClientGroup;
        
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Register
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public void RegisterFromFile(string path)
        {
            
        }

        public void RegisterFromString(string @string)
        {
            
        }

        public void RegisterFromList(List<InputSettingBase> informationMap, bool autoClear = true)
        {
            for (int i = 0; i != informationMap.Count; i++)
            {
                RegisterSingle(informationMap[i]);
            }
            if (autoClear) informationMap.Clear();
        }

        public void RegisterSingle(InputSettingBase setting)
        {
            var id = GetStockIdInternal(setting.NameId);
            var map = new Map
            {
                Id = id,
                NameId = setting.NameId,
                DefaultSettings = setting
            };

            switch (setting)
            {
                case Settings.Axis1D axis1D:
                {
                    map.SettingType = Settings.EType.Axis1D;
                    break;
                }
                case Settings.Axis2D axis2D:
                {
                    map.SettingType = Settings.EType.Axis2D;
                    break;
                }
                case Settings.Push push:
                {
                    map.SettingType = Settings.EType.Push;
                    break;
                }
            }

            s_Maps[id] = map;
        }
        
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Get
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public int GetId(string inputMapName)
        {
            return s_MapsStringLookup[inputMapName];
        }

        public CustomInputAction Get(int id)
        {
            throw new NotImplementedException();
        }

        public ref Map GetMap(int id)
        {
            return ref s_Maps.RefGet(id);
        }

        public Map GetMap(string nameId)
        {
            return GetMap(s_MapsStringLookup[nameId]);
        }

        protected override void OnCreateManager(int capacity)
        {
            s_Maps = new FastDictionary<int, Map>();
            s_MapsStringLookup = new FastDictionary<string, int>();
            s_InputActions = new FastDictionary<int, List<CustomInputAction>>();
        }

        protected override void OnUpdate()
        {
            for (int i = 0; i != m_ClientGroup.Length; i++)
            {
                var client = m_ClientGroup.Clients[i];
                var clientInputManager = client.GetWorld().GetOrCreateManager<ClientInputManager>();
                clientInputManager.Update();
            }
        }

        protected override void OnDestroyManager()
        {
            OnClientDeviceUpdate = null;
            OnClientSettingsChanged = null;
            OnClientDeviceChanged = null;
            
            s_Maps.Clear();
            s_MapsStringLookup.Clear();
            s_InputActions.Clear();

            s_Maps = null;
            s_MapsStringLookup = null;
            s_InputActions = null;
        }
    }
    
    // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
    // Internals
    // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.  
    public partial class CInputManager
    {
        private int GetStockIdInternal(string nameId)
        {
            if (s_MapsStringLookup.ContainsKey(nameId))
            {
                throw new InputAlreadyRegisteredException();
            }
            
            s_MapsStringLookup[nameId] = s_Maps.Count;
            
            return s_Maps.Count;
        }

        internal void CallOnClientDeviceUpdate(ClientInputManager origin)
        {
            OnClientDeviceUpdate?.Invoke(origin);
        }
        
        internal void CallOnClientSettingsChanged(ClientInputManager origin)
        {
            OnClientSettingsChanged?.Invoke(origin);
        }
        
        internal void CallOnClientDeviceChanged(ClientInputManager origin)
        {
            OnClientDeviceChanged?.Invoke(origin);
        }
    }

    // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
    // Statics
    // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.    
    public partial class CInputManager
    {
        private static FastDictionary<string, int> s_MapsStringLookup;
        private static FastDictionary<int, Map> s_Maps;
        private static FastDictionary<int, List<CustomInputAction>> s_InputActions;
    }
    
    // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
    // Sub-classes
    // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.  
    public partial class CInputManager
    {
        public class CustomInputAction
        {
            internal InputAction InputAction;

            public readonly int InputId;
            public readonly ClientEntity ClientEntity;
            
            /// <summary>
            /// Replace the standard inputsystem start/cancel event by a custom one
            /// </summary>
            public bool HackyStartCancel;

            public InputAction.Phase     Phase                => InputAction.phase;
            public InputControl          LastTriggerControl   => InputAction.lastTriggerControl;
            public double                LastTriggerTime      => InputAction.lastTriggerTime;
            public double                LastTriggerStartTime => InputAction.lastTriggerStartTime;
            public double                LastTriggerDuration  => InputAction.lastTriggerDuration;
            public InputBinding          LastTriggerBinding   => InputAction.lastTriggerBinding;
            public IInputBindingModifier LastTriggerModifier  => InputAction.lastTriggerModifier;

            public event Action<InputAction.CallbackContext, ClientEntity, int> Started;
            public event Action<InputAction.CallbackContext, ClientEntity, int> Cancelled;
            public event Action<InputAction.CallbackContext, ClientEntity, int> Performed;

            internal void SwitchAction(InputAction newAction)
            {
                if (newAction != InputAction)
                {
                    if (InputAction != null)
                    {
                        InputAction.started   -= OnStart;
                        InputAction.cancelled -= OnCancel;
                        InputAction.performed -= OnPerform;
                        
                        InputAction.Disable();
                    }

                    InputAction = newAction;

                    if (!HackyStartCancel)
                    {
                        InputAction.started   += OnStart;
                        InputAction.cancelled += OnCancel;
                    }

                    InputAction.performed += OnPerform;
                    
                    InputAction.Enable();
                }
            }
            
            public CustomInputAction(int inputId, ClientEntity attachedClientEntity)
            {
                InputId = inputId;
                ClientEntity = attachedClientEntity;
            }

            internal void OnStart(InputAction.CallbackContext context)
            {
                Started?.Invoke(context, ClientEntity, InputId);
            }

            internal void OnCancel(InputAction.CallbackContext context)
            {
                Cancelled?.Invoke(context, ClientEntity, InputId);
            }

            internal void OnPerform(InputAction.CallbackContext context)
            {
                if (context.GetValue<float>() != 0)
                    OnStart(context);
                else
                    OnCancel(context);
                
                Performed?.Invoke(context, ClientEntity, InputId);
            }
        }

        public struct Map : IEquatable<Map>
        {
            public string NameId;
            public int Id;
            
            public InputSettingBase DefaultSettings;

            public Settings.EType SettingType;

            //< -------- -------- -------- -------- -------- -------- -------- ------- //
            // IEquatable methods
            //> -------- -------- -------- -------- -------- -------- -------- ------- //
            public bool Equals(Map other)
            {
                return Id == other.Id;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is Map && Equals((Map) obj);
            }

            public override int GetHashCode()
            {
                return Id;
            }
        }
        
        public class InputAlreadyRegisteredException : Exception
        {
        }
    }
}
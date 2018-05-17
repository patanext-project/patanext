using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Packages.pack.guerro.shared.Scripts.Clients;
using Packages.pack.guerro.shared.Scripts.Utilities;
using Packages.pack.guerro.shared.Scripts.Utilities.ECSManager;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Experimental.Input.Controls;
using UnityEngine.Experimental.Input.LowLevel;
using UnityEngine.Experimental.Input.Utilities;
using UnityEngine.Profiling;

namespace Packet.Guerro.Shared.Inputs
{
    public partial class ClientInputManager : ClientComponentSystem
    {
        private static string s_LayoutKeyboard = "<keyboard>",
                              s_LayoutGamepad  = "<gamepad>",
                              s_LayoutMouse    = "<mouse>";

        private static Type s_TypePush   = typeof(CInputManager.Result.Push),
                            s_TypeAxis1D = typeof(CInputManager.Result.Axis1D),
                            s_TypeAxis2D = typeof(CInputManager.Result.Axis2D);

        private FastDictionary<string, CInputManager.Map> m_clientSettings
            = new FastDictionary<string, CInputManager.Map>();

        public IReadOnlyDictionary<string, CInputManager.Map> ClientSettings;

        private InputDevice m_ActiveDevice;
        public InputDevice ActiveDevice
        {
            get { return m_ActiveDevice; }
            set
            {
                if (value is Gamepad gamepad)
                {
                    m_DeviceType = 0;
                    m_CachedGamepad = gamepad;
                }

                if (value is Keyboard keyboard)
                {
                    m_DeviceType = 1;
                    m_CachedKeyboard = keyboard;
                }

                if (m_ActiveDevice != value)
                {
                    m_ActiveDevice = value;
                    
                    UpdateInputActionsInternal();
                    m_InputManager.CallOnClientDeviceChanged(this);
                }
            }
        }

        [Inject] private CInputManager m_InputManager;

        public ref CInputManager.Map GetMap(int inputId)
        {
            ref var map = ref m_InputManager.GetMap(inputId);
            if (CurrentSettingsIsValid(ref map, ref map))
            {
            }

            return ref map;
        }
        
        /*
         * Generate more garbage and boxing by casting/uncasting...
         */
        public TInputResult UnoptimizedGet<TInputResult>(int inputId)
            where TInputResult : CInputManager.IInputResult
        {
            var type = typeof(TInputResult);
            ref var map = ref GetMap(inputId);
            
            switch (map.DefaultSettings)
            {
                case CInputManager.Settings.Push push when type == s_TypePush:
                {
                    return (TInputResult) (object) ProcessPushInternal(push);
                }
                case CInputManager.Settings.Axis1D axis1D when type == s_TypeAxis1D:
                {
                    return (TInputResult) (object) ProcessAxis1DInternal(axis1D);
                }
                case CInputManager.Settings.Axis2D axis2D when type == s_TypeAxis2D:
                {
                    return (TInputResult) (object) ProcessAxis2DInternal(axis2D);
                }
            }

            return default(TInputResult);
        }

        public CInputManager.Result.Push GetPush(int inputId)
        {
            ref var map = ref GetMap(inputId);
            return ProcessPushInternal((CInputManager.Settings.Push)map.DefaultSettings);
        }
        
        public CInputManager.Result.Axis1D GetAxis1D(int inputId)
        {
            ref var map = ref GetMap(inputId);
            return ProcessAxis1DInternal((CInputManager.Settings.Axis1D)map.DefaultSettings);
        }
        
        public CInputManager.Result.Axis2D GetAxis2D(int inputId)
        {
            ref var map = ref GetMap(inputId);
            return ProcessAxis2DInternal((CInputManager.Settings.Axis2D)map.DefaultSettings);
        }

        //
        // Pre-process
        //
        private CInputManager.Result.Push ProcessPushInternal(CInputManager.Settings.Push push)
        {
            var layout = GetActiveLayout();

            var innerList    = push.GetLayout(layout);
            var inputControl = GetValueAndControl(ActiveDevice, innerList, out var value);

            return new CInputManager.Result.Push()
            {
                Value = value
            };
        }

        private CInputManager.Result.Axis1D ProcessAxis1DInternal(CInputManager.Settings.Axis1D axis1D)
        {
            var layout = GetActiveLayout();

            var innerList  = axis1D.GetLayout(layout);
            var finalValue = 0f;
            for (int i = 0; i != 2; i++) //< 1 Dimension, so 2 values to get
            {
                var internedString = InputDimension.GetDimensionStringId(i);
                var inputControl = GetValueAndControl(ActiveDevice, innerList[internedString], out var value);
                
                if (i % 2 == 0) finalValue -= value;
                else finalValue            += value;
            }

            return new CInputManager.Result.Axis1D()
            {
                Value = finalValue
            };
        }
        
        private CInputManager.Result.Axis2D ProcessAxis2DInternal(CInputManager.Settings.Axis2D axis2D)
        {
            var layout = GetActiveLayout();

            var innerList  = axis2D.GetLayout(layout);
            var finalValue = new Vector2();
            for (int i = 0, dimension = 0; i != 4; i++) //< 1 Dimension, so 2 values to get
            {
                var internedString = InputDimension.GetDimensionStringId(i);

                var inputControl = GetValueAndControl(ActiveDevice, innerList[internedString], out var value);
                if (i % 2 == 0) finalValue[dimension] -= value;
                else
                {
                    finalValue[dimension] += value;

                    dimension++;
                }
            }

            return new CInputManager.Result.Axis2D()
            {
                Value = finalValue
            };
        }

        public string GetActiveLayout()
        {
            var layout = string.Empty;
            if (ActiveDevice is Keyboard)
                layout = s_LayoutKeyboard;
            if (ActiveDevice is Gamepad)
                layout = s_LayoutGamepad;
            ;
            return layout;
        }

        public InputControl GetValueAndControl(InputDevice device, ReadOnlyCollection<string> paths, out float value)
        {
            float        highestDistanceToZero = 0f, finalValue = 0f;
            InputControl inputControl          = null;
            var          length                = paths.Count;

            /*
            * So we iterate throught the list of the controls
            * The return contro is based on the higher value we get
            * from it.
            */
            for (int i = 0; i != length; i++)
            {
                ref var result = ref GetControlAndValueInternal(device, paths[i]);
                if (result.Control != null)
                {
                    if (math.distance(result.Value, 0) > highestDistanceToZero)
                    {
                        highestDistanceToZero = math.distance(result.Value, 0);
                        finalValue            = result.Value;
                        inputControl          = result.Control;
                    }
                }
            }

            value = finalValue;

            return inputControl;
        }

        private bool CurrentSettingsIsValid(ref CInputManager.Map inputMap, ref CInputManager.Map clientSettings)
        {
            // Check if we have the key first
            CInputManager.Map ourSettings = default(CInputManager.Map);
            if (m_clientSettings.RefFastTryGet(inputMap.NameId, ref ourSettings))
            {
                // Check the type
                if (inputMap.SettingType == ourSettings.SettingType)
                {
                    clientSettings = ourSettings;
                    return true;
                }
            }

            return false;
        }

        private void UpdateInputActionsInternal()
        {
            foreach (var action in m_CachedInputActions)
            {
                var val      = action.Value;
                var map      = GetMap(action.Value.CustomAction.InputId);
                var layout   = GetActiveLayout();
                var settings = map.DefaultSettings;

                var needUpdate = val.Device != m_ActiveDevice;

                var bindingCount    = val.CustomAction.InputAction.bindings.Count;
                var correctBindings = 0;
                foreach (var control in val.CustomAction.InputAction.bindings)
                {
                    if (settings is CInputManager.Settings.Push push)
                    {
                        var innerList = push.RWDefaults[layout];

                        // If players don't make change every 0.1s, performance should be fine
                        if (innerList.Any(innerValue => control.path == innerValue))
                        {
                            correctBindings++;
                        }
                    }
                }

                if (bindingCount != correctBindings)
                    needUpdate = true;

                if (needUpdate)
                {
                    val.Device = m_ActiveDevice;
                    
                    var inputAction = new InputAction();
                    SetInputActionBindingsInternal(inputAction, settings);
                    val.CustomAction.SwitchAction(inputAction);
                }
            }
        }

        private void SetInputActionBindingsInternal(InputAction inputAction, CInputManager.InputSettingBase settings)
        {
            var wasEnabled = inputAction.enabled;
            inputAction.Disable();

            var layout = GetActiveLayout();
            if (settings is CInputManager.Settings.Push push)
            {
                var innerList = push.RWDefaults[layout];
                for (int i = 0; i != innerList.Length; i++)
                {
                    var path = $"{m_ActiveDevice.name}/{innerList[i]}";
                    if (path.StartsWith("/"))
                        path = path.Remove(0, 1);

                    if (inputAction.bindings.Count >= innerList.Length)
                        inputAction.ApplyBindingOverride(i, path);
                    else
                        inputAction.AddBinding(path);
                }
            }

            foreach (var binding in inputAction.bindings)
            {
                Debug.Log(inputAction.bindings.Count + ", " + binding.path);
            }

            if (wasEnabled) inputAction.Enable();
        }
    }

    public partial class ClientInputManager
    {
        protected override void OnCreateManager(int capacity)
        {
            InputSystem.onEvent += OnInputEvent;
            
            // Set default device
            m_ActiveDevice = Keyboard.current;
            m_DeviceType = 1;
            m_CachedKeyboard = Keyboard.current;
        }

        protected override void OnUpdate()
        {
            
        }

        protected override void OnDestroyManager()
        {
            InputSystem.onEvent -= OnInputEvent;
        }

        private void OnInputEvent(InputEventPtr ev)
        {
            var device = InputSystem.TryGetDeviceById(ev.deviceId);
            if (device == ActiveDevice)
                m_InputManager.CallOnClientDeviceUpdate(this);
        }

        /// <summary>
        /// Only working with push inputs
        /// </summary>
        /// <param name="inputId"></param>
        /// <param name="???"></param>
        /// <returns></returns>
        public CInputManager.CustomInputAction CreateInputAction(int inputId)
        {
            Debug.Log($"Going to register id: {inputId}");
            
            // todo: See if this should return an exception or null instead?
            if (m_CachedInputActions.ContainsKey(inputId))
                return GetInputAction(inputId);
            
            var cache = new CacheInputAction();
            cache.Device = m_ActiveDevice;
            cache.Version = 0;
            cache.CustomAction = new CInputManager.CustomInputAction(inputId, ClientWorld.Client);
              
            var inputAction = new InputAction();
            SetInputActionBindingsInternal(inputAction, GetMap(inputId).DefaultSettings);
            cache.CustomAction.SwitchAction(inputAction);

            m_CachedInputActions[m_CachedInputActions.Count] = cache;

            return cache.CustomAction;
        }

        public CInputManager.CustomInputAction GetInputAction(int actionId)
        {
            var inputAction = default(CacheInputAction);
            if (!m_CachedInputActions.RefFastTryGet(actionId, ref inputAction))
            {
                throw new KeyNotFoundException();
            }

            return inputAction.CustomAction;
        }
    }

    public partial class ClientInputManager
    {
        private struct CacheInputAction
        {
            public InputDevice Device;
            public CInputManager.CustomInputAction CustomAction;
            /// <summary>
            /// To match the client settings, current device, we verify the version.
            /// </summary>
            public int Version;
        }
        
        private struct InternalResult
        {
            public InputControl Control;
            public float  Value;
            public int FrameCount;

            public InternalResult(InputControl control, float value)
            {
                Control = control;
                Value = value;
                
                FrameCount = UpdateTimeManager.FrameCount;
            }
        }

        private int m_DeviceType = 0;
        private Gamepad m_CachedGamepad;
        private Keyboard m_CachedKeyboard;
        
        private static string s_dpadleft = "dpad/left";
        private static string s_dpadright = "dpad/right";
        private static string s_dpaddown = "dpad/down";
        private static string s_dpadup = "dpad/up";
        
        private static string s_buttonWest = "buttonWest";
        private static string s_buttonEast = "buttonEast";
        private static string s_buttonSouth = "buttonSouth";
        private static string s_buttonNorth = "buttonNorth";
        
        private static string s_space  = "space";

        private static string s_leftArrow = "leftArrow";
        private static string s_rightArrow = "rightArrow";
        private static string s_downArrow = "downArrow";
        private static string s_upArrow = "upArrow";        
        
        private static string s_numpad0 = "numpad0";
        private static string s_numpad1 = "numpad1";
        private static string s_numpad2 = "numpad2";
        private static string s_numpad3 = "numpad3";
        private static string s_numpad4 = "numpad4";
        private static string s_numpad5 = "numpad5";
        private static string s_numpad6 = "numpad6";
        private static string s_numpad7 = "numpad7";
        private static string s_numpad8 = "numpad8";
        private static string s_numpad9 = "numpad9";

        private FastDictionary<int, InternalResult> m_CachedResults
            = new FastDictionary<int, InternalResult>();
        
        private FastDictionary<int, CacheInputAction> m_CachedInputActions
             = new FastDictionary<int, CacheInputAction>();

        private ref InternalResult CacheInternal(InputControl control, float value)
        {            
            var hash = control.name.GetHashCode();
            var result = new InternalResult(control, value);
            m_CachedResults[hash] = result;
            return ref m_CachedResults.RefGet(hash);
        }

        private InternalResult cachedResult;
        
        private ref InternalResult GetControlAndValueInternal(InputDevice device, string path)
        {
            if (m_CachedResults.RefFastTryGet(path.GetHashCode(), ref cachedResult))
            {
                if (cachedResult.FrameCount == UpdateTimeManager.FrameCount)
                {
                    return ref cachedResult;
                }
            }

            if (m_DeviceType == 0)
            {
                var gamepad = m_CachedGamepad;
                
                if (path == s_dpadleft) return ref CacheInternal(gamepad.dpad.left, gamepad.dpad.left.ReadValue());
                if (path == s_dpadright) return ref CacheInternal(gamepad.dpad.right, gamepad.dpad.right.ReadValue());
                if (path == s_dpaddown) return ref CacheInternal(gamepad.dpad.down, gamepad.dpad.down.ReadValue());
                if (path == s_dpadup) return ref CacheInternal(gamepad.dpad.up, gamepad.dpad.up.ReadValue());
                
                if (path == s_buttonWest) return ref CacheInternal(gamepad.buttonWest, gamepad.buttonWest.ReadValue());
                if (path == s_buttonEast) return ref CacheInternal(gamepad.buttonEast, gamepad.buttonEast.ReadValue());
                if (path == s_buttonSouth) return ref CacheInternal(gamepad.buttonSouth, gamepad.buttonSouth.ReadValue());
                if (path == s_buttonNorth) return ref CacheInternal(gamepad.buttonNorth, gamepad.buttonNorth.ReadValue());
            }

            if (m_DeviceType == 1)
            {
                var keyboard = m_CachedKeyboard;
                
                if (path == s_space) return ref CacheInternal(keyboard.spaceKey, keyboard.spaceKey.ReadValue());
                
                if (path == s_leftArrow) return ref CacheInternal(keyboard.leftArrowKey, keyboard.leftArrowKey.ReadValue());
                if (path == s_leftArrow) return ref CacheInternal(keyboard.leftArrowKey, keyboard.leftArrowKey.ReadValue());
                if (path == s_buttonSouth) return ref CacheInternal(keyboard.rightArrowKey, keyboard.rightArrowKey.ReadValue());
                if (path == s_buttonNorth) return ref CacheInternal(keyboard.downArrowKey, keyboard.downArrowKey.ReadValue());
                if (path == s_buttonNorth) return ref CacheInternal(keyboard.upArrowKey, keyboard.upArrowKey.ReadValue());
                
                if (path == s_numpad0) return ref CacheInternal(keyboard.numpad0Key, keyboard.numpad0Key.ReadValue());
                if (path == s_numpad1) return ref CacheInternal(keyboard.numpad1Key, keyboard.numpad1Key.ReadValue());
                if (path == s_numpad2) return ref CacheInternal(keyboard.numpad2Key, keyboard.numpad2Key.ReadValue());
                if (path == s_numpad3) return ref CacheInternal(keyboard.numpad3Key, keyboard.numpad3Key.ReadValue());
                if (path == s_numpad4) return ref CacheInternal(keyboard.numpad4Key, keyboard.numpad4Key.ReadValue());
                if (path == s_numpad5) return ref CacheInternal(keyboard.numpad5Key, keyboard.numpad5Key.ReadValue());
                if (path == s_numpad6) return ref CacheInternal(keyboard.numpad6Key, keyboard.numpad6Key.ReadValue());
                if (path == s_numpad7) return ref CacheInternal(keyboard.numpad7Key, keyboard.numpad7Key.ReadValue());
                if (path == s_numpad8) return ref CacheInternal(keyboard.numpad8Key, keyboard.numpad8Key.ReadValue());
                if (path == s_numpad9) return ref CacheInternal(keyboard.numpad9Key, keyboard.numpad9Key.ReadValue());

                // Default
                var array = keyboard.allControls;
                for (int i = 0; i != array.Count; i++)
                {
                    var inputControl = array[i];
                    if (inputControl.name == path)
                        return ref CacheInternal(inputControl, (float)inputControl.ReadValueAsObject());
                }
            }
            
            // Default
            var control = device[path];
            return ref CacheInternal(control, (float)control.ReadValueAsObject());
        }
    }
}
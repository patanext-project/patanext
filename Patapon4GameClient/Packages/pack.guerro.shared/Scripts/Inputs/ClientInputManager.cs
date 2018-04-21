using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Packages.pack.guerro.shared.Scripts.Clients;
using Packages.pack.guerro.shared.Scripts.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Experimental.Input.Controls;
using UnityEngine.Experimental.Input.Utilities;
using UnityEngine.Profiling;

namespace Packet.Guerro.Shared.Inputs
{
    public partial class ClientInputManager : ClientScriptBehaviourManager
    {
        private static string s_LayoutKeyboard = "<keyboard>",
                              s_LayoutGamepad  = "<gamepad>",
                              s_LayoutMice     = "<mice>";

        private static Type s_TypePush   = typeof(CInputManager.Result.Push),
                            s_TypeAxis1D = typeof(CInputManager.Result.Axis1D),
                            s_TypeAxis2D = typeof(CInputManager.Result.Axis2D);

        private FastDictionary<string, CInputManager.Map> m_clientSettings
            = new FastDictionary<string, CInputManager.Map>();

        public IReadOnlyDictionary<string, CInputManager.Map> ClientSettings;

        public InputDevice ActiveDevice;

        [Inject] private CInputManager m_InputManager;

        public TInputResult Get<TInputResult>(int inputId)
            where TInputResult : CInputManager.IInputResult
        {
            var map = m_InputManager.GetMap(inputId);
            if (CurrentSettingsIsValid(map, ref map))
            {
                // do nothing lol
            }

            if (map.DefaultSettings.ResultType != typeof(TInputResult))
            {
                Debug.LogError(
                    $"Wrong type of 'InputResult' ({map.DefaultSettings.ResultType.Name} against {typeof(TInputResult).Name})");

                return default(TInputResult);
            }

            var type = typeof(TInputResult);
            switch (map.DefaultSettings)
            {
                // This is ugly, how I could do that better?
                case CInputManager.Settings.Push push when type == s_TypePush:
                {
                    // UGLY, Are there any way to prevent boxing?
                    // I could emit dynamic code, but it would be really ugly (but more performant).
                    var value = (TInputResult) (object) ProcessPushInternal(push);

                    return value;
                }
                case CInputManager.Settings.Axis1D axis1D when type == s_TypeAxis1D:
                {
                    // UGLY, Are there any way to prevent boxing?
                    // I could emit dynamic code, but it would be really ugly (but more performant).
                    var value = (TInputResult) (object) ProcessAxis1DInternal(axis1D);
                    
                    return value;
                }
                case CInputManager.Settings.Axis2D axis2D when type == s_TypeAxis2D:
                {
                    // UGLY, Are there any way to prevent boxing?
                    // I could emit dynamic code, but it would be really ugly (but more performant).
                    var value = (TInputResult) (object) ProcessAxis2DInternal(axis2D);
                    
                    return value;
                }
            }

            return default(TInputResult);
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
                var controlValueTuple = GetControlAndValue(device, paths[i]);
                if (controlValueTuple.control != null)
                {
                    if (math.distance(controlValueTuple.value, 0) > highestDistanceToZero)
                    {
                        highestDistanceToZero = math.distance(controlValueTuple.value, 0);
                        finalValue            = controlValueTuple.value;
                        inputControl          = controlValueTuple.control;
                    }
                }
            }

            value = finalValue;

            return inputControl;
        }

        private bool CurrentSettingsIsValid(CInputManager.Map inputMap, ref CInputManager.Map clientSettings)
        {
            // Check if we have the key first
            if (m_clientSettings.FastTryGet(inputMap.NameId, out var ourSettings))
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

        protected override void OnUpdate()
        {

        }
    }

    public partial class ClientInputManager
    {
        private (InputControl control, float value) GetControlAndValue(InputDevice device, string path)
        {
            if (device is Gamepad gamepad)
            {
                if (path == "dpad/left") return (gamepad.dpad.left, gamepad.dpad.left.ReadValue());
                if (path == "dpad/right") return (gamepad.dpad.right, gamepad.dpad.right.ReadValue());
                if (path == "dpad/down") return (gamepad.dpad.down, gamepad.dpad.down.ReadValue());
                if (path == "dpad/up") return (gamepad.dpad.up, gamepad.dpad.up.ReadValue());
                
                if (path == "buttonWest") return (gamepad.buttonWest, gamepad.buttonWest.ReadValue());
                if (path == "buttonEast") return (gamepad.buttonEast, gamepad.buttonEast.ReadValue());
                if (path == "buttonSouth") return (gamepad.buttonSouth, gamepad.buttonSouth.ReadValue());
                if (path == "buttonNorth") return (gamepad.buttonNorth, gamepad.buttonNorth.ReadValue());
            }

            if (device is Keyboard keyboard)
            {
                foreach (var inputControl in keyboard.allControls)
                {
                    if (inputControl.name == path)
                        return (inputControl, (float)inputControl.ReadValueAsObject());
                }
            }
            
            // Default
            var control = device[path];
            return (control, (float)control.ReadValueAsObject());
        }
    }
}
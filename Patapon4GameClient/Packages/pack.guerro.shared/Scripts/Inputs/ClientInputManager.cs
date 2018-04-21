using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Packages.pack.guerro.shared.Scripts.Clients;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Experimental.Input.Controls;
using UnityEngine.Experimental.Input.Utilities;

namespace Packet.Guerro.Shared.Inputs
{
    public class ClientInputManager : ClientScriptBehaviourManager
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

            if (map.UnknowSetting.ResultType != typeof(TInputResult))
            {
                Debug.LogError(
                    $"Wrong type of 'InputResult' ({map.UnknowSetting.ResultType.Name} against {typeof(TInputResult).Name})");

                return default(TInputResult);
            }

            var type = typeof(TInputResult);
            switch (map.UnknowSetting)
            {
                // This is ugly, how I could do that better?
                case CInputManager.Settings.Push push when type == s_TypePush:
                {
                    // UGLY, Are there any way to prevent boxing?
                    // I could emit dynamic code, but it would be really ugly (but more performant).
                    return (TInputResult) (object) ProcessPushInternal(push);
                }
                case CInputManager.Settings.Axis1D axis1D when type == s_TypeAxis1D:
                {
                    // UGLY, Are there any way to prevent boxing?
                    // I could emit dynamic code, but it would be really ugly (but more performant).
                    return (TInputResult) (object) ProcessAxis1DInternal(axis1D);
                }
                case CInputManager.Settings.Axis2D axis2D when type == s_TypeAxis2D:
                {
                    // UGLY, Are there any way to prevent boxing?
                    // I could emit dynamic code, but it would be really ugly (but more performant).
                    return (TInputResult) (object) ProcessAxis2DInternal(axis2D);
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

            var innerList    = push.Defaults[layout];
            var inputControl = GetValueAndControl(ActiveDevice, innerList, out var value);

            return new CInputManager.Result.Push()
            {
                Value = value
            };
        }

        private CInputManager.Result.Axis1D ProcessAxis1DInternal(CInputManager.Settings.Axis1D axis1D)
        {
            var layout = GetActiveLayout();

            var innerList  = axis1D.Defaults[layout];
            var finalValue = 0f;
            for (int i = 0; i != 2; i++) //< 1 Dimension, so 2 values to get
            {
                var internedString = GetDimensionStringId(i);

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

            var innerList  = axis2D.Defaults[layout];
            var finalValue = new Vector2();
            for (int i = 0, dimension = 0; i != 4; i++) //< 1 Dimension, so 2 values to get
            {
                var internedString = GetDimensionStringId(i);

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

        public InternedString GetDimensionStringId(int index)
        {
            if (index == 0) return new InternedString("-x");
            if (index == 1) return new InternedString("+x");
            if (index == 2) return new InternedString("-y");
            if (index == 3) return new InternedString("+y");
            return new InternedString("?d");
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
                var asAxisButton = device[paths[i]] as AxisControl;
                // todo...
                var asKeyButton = device[paths[i]] as KeyControl;

                if (asAxisButton != null)
                {
                    var val = asAxisButton.ReadValue();

                    if (math.distance(val, 0) > highestDistanceToZero)
                    {
                        highestDistanceToZero = math.distance(val, 0);
                        finalValue            = val;
                        inputControl          = asAxisButton;
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
                if (inputMap.GetType() == ourSettings.GetType())
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
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Packages.pack.guerro.shared.Scripts.Modding;
using Unity.Entities;
using UnityEngine;

namespace Packet.Guerro.Shared.Inputs
{
    public class ModInputManager : ModComponentSystem
    {
        [Inject] private CInputManager m_InputManager;

        private FastDictionary<string, CInputManager.InputSettingBase> GetInformationFromJson(string json)
        {
            var dico       = new FastDictionary<string, CInputManager.InputSettingBase>();
            var jsonObject = JObject.Parse(json);
            foreach (var id in jsonObject)
            {
                CInputManager.InputSettingBase setting = null;
                var valueArray = id.Value as JObject;
                foreach (var property in valueArray)
                {
                    var propertyName  = property.Key;
                    var propertyValue = property.Value.Value<string>();
                    if (propertyName == "type")
                    {
                        if (propertyValue == "push") setting   = new CInputManager.Settings.Push(id.Key);
                        if (propertyValue == "axis1d") setting = new CInputManager.Settings.Axis1D(id.Key);
                        if (propertyValue == "axis2d") setting = new CInputManager.Settings.Axis2D(id.Key);
                    }

                    if (setting == null)
                        throw new Exception($"An input setting ({id.Key}) was invalid.");

                    if (propertyName == "displayName")
                    {
                        setting.DisplayName = propertyValue;
                    }

                    if (propertyName == "translation")
                    {
                        setting.Translation = propertyValue;
                    }
                }

                dico[id.Key] = setting;
            }

            return dico;
        }

        private void RegisterDefaultsFromJson
        (
            string                                                 layout,
            string                                                 json,
            FastDictionary<string, CInputManager.InputSettingBase> informations
        )
        {
            var jsonObject = JObject.Parse(json);
            foreach (var id in jsonObject)
            {
                if (informations.ContainsKey(id.Key))
                {
                    // TODO: Refactor this, it's ugly, very ugly
                    if (informations[id.Key] is CInputManager.Settings.Push push)
                    {
                        var valueArray = id.Value as JObject;
                        foreach (var property in valueArray)
                        {
                            var propertyName  = property.Key;
                            var propertyValue = property.Value.Value<string>();
                            if (propertyName == "default")
                            {
                                push.RWDefaults[layout] =
                                    propertyValue
                                        .Replace(" ", string.Empty)
                                        .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                            }
                        }
                    }

                    if (informations[id.Key] is CInputManager.Settings.Axis1D axis1D)
                    {
                        var valueArray = id.Value as JObject;
                        foreach (var property in valueArray)
                        {
                            var propertyName  = property.Key;
                            var propertyValue = property.Value.Value<string>();
                            if (propertyName == "-x")
                            {
                                axis1D.RWDefaults[layout]["-x"] =
                                    propertyValue
                                        .Replace(" ", string.Empty)
                                        .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                            }

                            if (propertyName == "+x")
                            {
                                axis1D.RWDefaults[layout]["+x"] =
                                    propertyValue
                                        .Replace(" ", string.Empty)
                                        .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                            }
                        }
                    }

                    if (informations[id.Key] is CInputManager.Settings.Axis2D axis2D)
                    {
                        var valueArray = id.Value as JObject;
                        foreach (var property in valueArray)
                        {
                            var propertyName  = property.Key;
                            var propertyValue = property.Value.Value<string>();
                            if (propertyName == "-x")
                            {
                                axis2D.RWDefaults[layout]["-x"] =
                                    propertyValue
                                        .Replace(" ", string.Empty)
                                        .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                            }

                            if (propertyName == "+x")
                            {
                                axis2D.RWDefaults[layout]["+x"] =
                                    propertyValue
                                        .Replace(" ", string.Empty)
                                        .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                            }

                            if (propertyName == "-y")
                            {
                                axis2D.RWDefaults[layout]["-y"] =
                                    propertyValue
                                        .Replace(" ", string.Empty)
                                        .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                            }

                            if (propertyName == "+y")
                            {
                                axis2D.RWDefaults[layout]["+y"] =
                                    propertyValue
                                        .Replace(" ", string.Empty)
                                        .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                            }
                        }
                    }
                }
            }
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Register
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        /*
         * Omg, it's ugly
         */
        public void RegisterFromFile(string path, bool customPath = false)
        {
            if (customPath)
                throw new NotImplementedException();

            var directoryPath = ModWorld.Mod.StreamingPath + "/Inputs/";
            var filePath = directoryPath + path + ".json";
            if (File.Exists(filePath))
            {
                var mainFile = File.ReadAllText(filePath);
                var layoutFiles = new FastDictionary<string, string>();
                var correspondingFiles = Directory.GetFiles(directoryPath, $"{path}.layout.*.json");
                foreach (var file in correspondingFiles)
                {
                    var fileName = Path.GetFileName(file);
                    layoutFiles
                        [
                            "<" + fileName.Replace($"{path}.layout.", string.Empty)
                                    .Replace(".json", string.Empty) + ">"
                        ]
                        = File.ReadAllText(file);
                }

                var informations = GetInformationFromJson(mainFile);
                foreach (var layoutText in layoutFiles)
                {
                    RegisterDefaultsFromJson(layoutText.Key, layoutText.Value, informations);
                }

                var list = informations.Values.ToList();
                foreach (var value in list)
                {
                    RegisterSingle(value);
                }
                
                informations.Clear();
                layoutFiles.Clear();
            }
            else
            {
                Debug.LogError($"File: \"{filePath}\" not found!");
            }
        }

        public void RegisterFromString(string @string)
        {

        }

        public void RegisterFromList(List<CInputManager.InputSettingBase> informationMap, bool autoClear = false)
        {
            for (int i = 0; i != informationMap.Count; i++)
            {
                RegisterSingle(informationMap[i]);
            }

            if (autoClear) informationMap.Clear();
        }

        public void RegisterSingle(CInputManager.InputSettingBase setting)
        {
            if (!setting.NameId.StartsWith(ModWorld.Mod.NameId + "."))
            {
                setting.NameId = ModWorld.Mod.NameId + "." + setting.NameId;
            }
            
            setting.Refresh();
            
            Debug.Log("New input added: " + setting.NameId);

            m_InputManager.RegisterSingle(setting);
        }
        
        public int GetId(string s)
        {
            return m_InputManager.GetId(ModWorld.Mod.NameId + "." + s);
        }

        protected override void OnUpdate()
        {

        }
    }
}
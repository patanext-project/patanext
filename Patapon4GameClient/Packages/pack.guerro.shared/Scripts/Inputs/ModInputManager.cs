using System;
using System.Collections.Generic;
using System.IO;
using Packages.pack.guerro.shared.Scripts.Modding;
using Unity.Entities;
using UnityEngine;

namespace Packet.Guerro.Shared.Inputs
{
    public class ModInputManager : ModScriptBehaviourManager
    {
        [Inject] private CInputManager m_InputManager;

        private FastDictionary<string, string> GetInformationFromJson(string json)
        {
            var dico = new FastDictionary<string, string>();
            JsonUtility.
        }
        
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Register
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
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
                    layoutFiles[file] = File.ReadAllText(file);
                }

                var informations = GetInformationFromJson(mainFile);
                
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
            
            Debug.Log("New input added: " + setting.NameId);

            m_InputManager.RegisterSingle(setting);
        }

        protected override void OnUpdate()
        {

        }
    }
}
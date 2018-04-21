using Packages.pack.guerro.shared.Scripts.Modding;
using UnityEditor;
using UnityEngine;

namespace P4.Default
{
    public class Bootstrap : CModBootstrap
    {
        protected override void OnRegister()
        {
            Debug.Log(ModInfo.DisplayName); //< Should log "Patapon Default Assets"
            
            Debug.Log(CModInfo.GetRunningMod().DisplayName); //< Should also log "Patapon Default Assets" (you might want to know how, hm?)
        }

        protected override void OnUnregister()
        {
            
        }
    }
}
using UnityEngine;

namespace Packages.pack.guerro.shared.Scripts.Modding
{
    /// <summary>
    /// This is the entry point for mods.
    /// </summary>
    public abstract class CModBootstrap
    {
        public CModInfo ModInfo { get; private set; }

        internal void RegisterInternal()
        {
            Debug.Log($"[MOD] New mod {ModInfo.DisplayName} ({ModInfo.NameId}) was registered!");
            
            OnRegister();
        }

        protected abstract void OnRegister();
        protected abstract void OnUnregister();

        internal void SetModInfoInternal(CModInfo modInfo)
        {
            ModInfo = modInfo;
        }
    }
}
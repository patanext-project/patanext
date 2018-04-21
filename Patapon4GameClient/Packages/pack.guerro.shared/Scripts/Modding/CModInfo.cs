using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.Entities;
using UnityEditor.Compilation;
using UnityEngine.Experimental.Input.Utilities;
using Assembly = System.Reflection.Assembly;

namespace Packages.pack.guerro.shared.Scripts.Modding
{
    public class CModInfo
    {
        private Assembly[] m_Assemblies;

        public int Id { get; }

        public SModInfoData Data { get; }

        public string DisplayName => Data.DisplayName;
        public string NameId => Data.NameId;

        public ReadOnlyArray<Assembly> AttachedAssemblies
            => new ReadOnlyArray<Assembly>(m_Assemblies);

        public CModInfo(SModInfoData data, int id)
        {
            Data = data;
            Id = id;
        }

        public static CModInfo GetRunningMod()
        {
            var assembly = Assembly.GetCallingAssembly();
            return CModManager.GetAssemblyMod(assembly);
        }
    }

    public partial class CModManager
    {
        public static CModInfo GetAssemblyMod(Assembly assembly)
        {
            return m_LoadedModsLookup[assembly];
        }
    }

    public partial class CModManager
    {
        private static List<CModInfo> m_LoadedMods = new List<CModInfo>();

        private static Dictionary<Assembly, CModInfo> m_LoadedModsLookup
            = new Dictionary<Assembly, CModInfo>();
        
        public static ReadOnlyCollection<CModInfo> LoadedMods 
            => new ReadOnlyCollection<CModInfo>(m_LoadedMods);
    }

    // todo: clean this, and set the base to ComponentSystem
    public partial class CModManager
    {
        public class InternalRegisteration
        {
            public bool IsRunning { get; private set; }

            internal InternalRegisteration()
            {
                IsRunning = false;
            }

            public void End()
            {
                IsRunning = false;
            }

            public void AddInternalPacket(string displayName, string nameId, Assembly[] assemblies)
            {
                RegisterModInternal(assemblies, new SModInfoData()
                {
                    DisplayName = displayName,
                    NameId = nameId,
                    IsIntegratedPacket = true
                });
            }
        }
        
        /// <summary>
        /// When this variable is at 1, we can't register internal packets anymore
        /// </summary>
        private static int s_RegisterCount = 0;

        private static InternalRegisteration s_CurrentInternalRegisteration;

        private static void RegisterModInternal(Assembly[] assemblies, SModInfoData data)
        {
            var modInfo = new CModInfo(data, m_LoadedMods.Count);
            
            m_LoadedMods.Add(modInfo);
            foreach (var assembly in assemblies)
            {
                m_LoadedModsLookup[assembly] = modInfo;
            }
            
            // Load if it's not an integrated packet...
            if (!data.IsIntegratedPacket)
            {
                // todo
            }
            else
            {
                // do nothing lol
            }
            
            // Call the bootstrappers...
            foreach (var assembly in assemblies)
            {
                var bootStrapperTypes = assembly.GetTypes().Where(t => 
                    t.IsSubclassOf(typeof(CModBootstrap)) && 
                    !t.IsAbstract && 
                    !t.ContainsGenericParameters && 
                    t.GetCustomAttributes(typeof(DisableAutoCreationAttribute), true).Length == 0);

                foreach (var bootstrapperType in bootStrapperTypes)
                {
                    var bootstrap = Activator.CreateInstance(bootstrapperType) as CModBootstrap;
                    bootstrap.SetModInfoInternal(modInfo);
                    bootstrap.RegisterInternal();
                }
            }
        }

        public static InternalRegisteration BeginInternalRegisteration()
        {
            if (s_RegisterCount != 0 || s_CurrentInternalRegisteration != null)
            {
                throw new Exception("You now cannot register new internal packets.");
            }

            s_RegisterCount++;

            return s_CurrentInternalRegisteration = new InternalRegisteration();
        }
    }
}
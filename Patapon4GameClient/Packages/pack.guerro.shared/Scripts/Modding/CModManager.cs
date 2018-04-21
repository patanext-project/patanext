using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Packages.pack.guerro.shared.Scripts.Modding;
using Unity.Entities;

namespace Packet.Guerro.Shared
{
    // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
    // Public Group
    // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
    public partial class CModManager : ComponentSystem
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        /// <summary>
        /// Get the Mod of the target assembly
        /// </summary>
        /// <param name="assembly">The target assembly</param>
        /// <returns>The mod or null if nothing is found</returns>
        public CModInfo GetAssemblyMod(Assembly assembly)
        {
            return m_LoadedModsLookup[assembly];
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Base methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        protected override void OnUpdate()
        {
            
        }
    }

    // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
    // Private and static fields group
    // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
    /*
     * We keep them static because we don't want to loose the values
     * per world.
     */
    public partial class CModManager
    {
        private static List<CModInfo> m_LoadedMods = new List<CModInfo>();

        private static Dictionary<Assembly, CModInfo> m_LoadedModsLookup
            = new Dictionary<Assembly, CModInfo>();
        
        /// <summary>
        /// Get all the running mods (does not allocate)
        /// </summary>
        public static ReadOnlyCollection<CModInfo> LoadedMods 
            => new ReadOnlyCollection<CModInfo>(m_LoadedMods);
    }

    // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
    // Internal group
    // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
    public partial class CModManager
    {
        public class InternalRegistration
        {
            public bool IsRunning { get; private set; }

            internal InternalRegistration()
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

        private static InternalRegistration s_CurrentInternalRegistration;

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

        /// <summary>
        /// Begin the system to register new internal packets.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">You cannot register new internal packets</exception>
        public static InternalRegistration BeginInternalRegistration()
        {
            if (s_RegisterCount != 0 || s_CurrentInternalRegistration != null)
            {
                throw new Exception("You now cannot register new internal packets.");
            }

            s_RegisterCount++;

            return s_CurrentInternalRegistration = new InternalRegistration();
        }
    }
}
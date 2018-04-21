using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Packet.Guerro.Shared;
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
            return World.Active.GetOrCreateManager<CModManager>().GetAssemblyMod(assembly);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using Packet.Guerro.Shared;
using Packet.Guerro.Shared.Inputs;
using Unity.Entities;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Experimental.Input.Utilities;
using Assembly = System.Reflection.Assembly;

namespace Packages.pack.guerro.shared.Scripts.Modding
{
    public class CModInfo
    {
        [AttributeUsage(AttributeTargets.Field)]
        public class InjectAttribute : Attribute {}
        
        private Assembly[] m_Assemblies;

        public int Id { get; }

        public SModInfoData Data { get; }
        public string StreamingPath => Application.streamingAssetsPath + "\\" + NameId;

        public string DisplayName => Data.DisplayName;
        public string NameId => Data.NameId;

        public ReadOnlyArray<Assembly> AttachedAssemblies
            => new ReadOnlyArray<Assembly>(m_Assemblies);

        public CModInfo(SModInfoData data, int id)
        {
            Data = data;
            Id = id;

            if (NameId.Contains('/')
                || NameId.Contains('\\')
                || NameId.Contains('?')
                || NameId.Contains(':')
                || NameId.Contains('|')
                || NameId.Contains('*')
                || NameId.Contains('<')
                || NameId.Contains('>'))
            {
                throw new Exception($"Name id {NameId} got invalid characters");
            }
            
            // Create the path to the project...
            if (!Directory.Exists(StreamingPath))
            Directory.CreateDirectory(StreamingPath);
        }

        public static CModInfo CurrentMod
        {
            get
            {
                var assembly = Assembly.GetCallingAssembly();
                return World.Active.GetOrCreateManager<CModManager>().GetAssemblyMod(assembly);
            }
        }

        public static ModWorld CurrentModWorld
        {
            get
            {
                var assembly = Assembly.GetCallingAssembly();
                return World.Active.GetOrCreateManager<CModManager>().GetAssemblyMod(assembly).GetWorld();
            }
        }
    }

    public static class CModInfoExtensions
    {
        public static ModWorld GetWorld(this CModInfo modInfo)
        {
            return World.Active.GetOrCreateManager<CModManager>().GetModWorld(modInfo);
        }

        public static ModInputManager GetInputManager(this CModInfo modInfo)
        {
            return modInfo.GetWorld().GetOrCreateManager<ModInputManager>();
        }
    }
}
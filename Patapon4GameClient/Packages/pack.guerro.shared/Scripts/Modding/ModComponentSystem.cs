using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using JetBrains.Annotations;
using Packages.pack.guerro.shared.Scripts.Clients;
using Packet.Guerro.Shared.Clients;
using Unity.Entities;
using UnityEngine;

namespace Packages.pack.guerro.shared.Scripts.Modding
{
    public abstract class ModComponentSystem : ComponentSystem
    {
	    public ModWorld ModWorld;

        internal void AddManagerForMod(ModWorld modWorld)
        {
	        ModWorld = modWorld;
        }
    }

    public class ModWorld
    {
        // Well, it somewhat copy the same code of Entity/World.cs
	    public CModInfo Mod { get; private set; }

	    [NotNull]
	    private static MethodInfo s_DestroyInstance;

	    [NotNull]
	    private static MethodInfo s_CreateInstance;
	    
        public IEnumerable<ModComponentSystem> BehaviourManagers =>
            new ReadOnlyCollection<ModComponentSystem>(m_BehaviourManagers);

        private List<ModComponentSystem> m_BehaviourManagers = new List<ModComponentSystem>();

        //@TODO: What about multiple managers of the same type...
        Dictionary<Type, ModComponentSystem> m_BehaviourManagerLookup =
            new Dictionary<Type, ModComponentSystem>();
	    
	    static FastDictionary<int, ModWorld> s_WorldsModIdLookup =
		    new FastDictionary<int, ModWorld>();

        int m_DefaultCapacity = 10;
        bool m_AllowGetManager = true;

        public int Version { get { return m_Version; } }
        int m_Version = 0;
	    
	    internal static readonly List<ModWorld> AllWorlds = new List<ModWorld>();

        int GetCapacityForType(Type type)
        {
            return m_DefaultCapacity;
        }

        public void SetDefaultCapacity(int value)
        {
            m_DefaultCapacity = value;
        }

	    static ModWorld()
	    {
		    // ReSharper disable AssignNullToNotNullAttribute
		    s_DestroyInstance = typeof(ScriptBehaviourManager)
			    .GetMethod("DestroyInstance", BindingFlags.NonPublic
			                                   | BindingFlags.Instance);
		    s_CreateInstance = typeof(ScriptBehaviourManager)
			    .GetMethod("CreateInstance", BindingFlags.NonPublic
			                                      | BindingFlags.Instance);

		    Debug.Assert(s_DestroyInstance != null, "s_DestroyInstance == null");
		    Debug.Assert(s_CreateInstance != null, "s_CreateInstance == null");
		    // ReSharper restore AssignNullToNotNullAttribute
	    }

	    public static ModWorld GetOrCreate(CModInfo modInfo)
	    {
		    if (s_WorldsModIdLookup.TryGetValue(modInfo.Id, out var world))
		    {
			    return world;
		    }

		    return new ModWorld(modInfo);
	    }

        public ModWorld(CModInfo modInfo)
        {
	        Mod = modInfo;
	        
	        s_WorldsModIdLookup[Mod.Id] = this;
	        AllWorlds.Add(this);
        }

        public bool IsCreated
        {
            get { return m_BehaviourManagers != null; }
        }

	    public static void DisposeAll()
	    {
		    while (AllWorlds.Count != 0)
			    AllWorlds[0].Dispose();
	    }

        public void Dispose()
		{
		    if (!IsCreated)
		        throw new System.ArgumentException("World is already disposed");
			
			if (AllWorlds.Contains(this))
				AllWorlds.Remove(this);

			// Destruction should happen in reverse order to construction
			m_BehaviourManagers.Reverse();

			m_AllowGetManager = false;
			foreach (var behaviourManager in m_BehaviourManagers)
			{
				try
				{
					DestroyManager(behaviourManager);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
		    
			m_BehaviourManagers.Clear();
			m_BehaviourManagerLookup.Clear();
			
			m_BehaviourManagers = null;
			m_BehaviourManagerLookup = null;
		}

	    //
	    // Internal
	    //
	    ModComponentSystem CreateManagerInternal (Type type, int capacity, object[] constructorArguments)
		{
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			if (!m_AllowGetManager)
				throw new ArgumentException("During destruction of a system you are not allowed to create more systems.");

			if (constructorArguments != null && constructorArguments.Length != 0)
			{
				var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				if (constructors.Length == 1 && constructors[0].IsPrivate)
					throw new MissingMethodException($"Constructing {type} failed because the constructor was private, it must be public.");
			}
#endif

		    m_AllowGetManager = true;
		    ModComponentSystem manager;
		    try
		    {
		        manager = Activator.CreateInstance(type, constructorArguments) as ModComponentSystem;

		    }
		    catch
		    {
		        m_AllowGetManager = false;
		        throw;
		    }

			m_BehaviourManagers.Add (manager);
		    AddTypeLookup(type, manager);

		    try
		    {
			    manager.AddManagerForMod(this);
		        CreateManagerExtraInternal(manager, capacity);

		    }
		    catch
		    {
		        RemoveManagerInteral(manager);
		        throw;
		    }

		    ++m_Version;
			return manager;
		}

	    ModComponentSystem GetExistingManagerInternal (Type type)
		{
#if ENABLE_UNITY_COLLECTIONS_CHECKS
		    if (!IsCreated)
		        throw new ArgumentException("During destruction ");
			if (!m_AllowGetManager)
				throw new ArgumentException("During destruction of a system you are not allowed to get or create more systems.");
#endif

			ModComponentSystem manager ;
			if (m_BehaviourManagerLookup.TryGetValue(type, out manager))
				return manager;

			return null;
		}

	    ModComponentSystem GetOrCreateManagerInternal (Type type)
		{
			var manager = GetExistingManagerInternal(type);

			return manager ?? CreateManagerInternal(type, GetCapacityForType(type), null);
		}

	    void AddTypeLookup(Type type, ModComponentSystem manager)
	    {
	        while (type != typeof(ScriptBehaviourManager))
	        {
	            if (!m_BehaviourManagerLookup.ContainsKey(type))
	                m_BehaviourManagerLookup.Add(type, manager);

	            type = type.BaseType;
	        }
	    }

	    void RemoveManagerInteral(ModComponentSystem manager)
		{
			if (!m_BehaviourManagers.Remove(manager))
				throw new ArgumentException($"manager does not exist in the world");
		    ++m_Version;

			var type = manager.GetType();
			while (type != typeof(ScriptBehaviourManager))
			{
			    if (m_BehaviourManagerLookup[type] == manager)
			    {
			        m_BehaviourManagerLookup.Remove(type);

			        foreach (var otherManager in m_BehaviourManagers)
			        {
			            if (otherManager.GetType().IsSubclassOf(type))
			                AddTypeLookup(otherManager.GetType(), otherManager);
			        }
			    }

				type = type.BaseType;
			}
		}

	    //
	    // Extra internal
	    //
	    void RemoveManagerExtraInternal(ModComponentSystem manager)
	    {
		    s_DestroyInstance.Invoke(manager, null);
	    }
	    
	    void CreateManagerExtraInternal(ModComponentSystem manager, int capacity)
	    {
		    s_CreateInstance.Invoke(manager, new object[]{ World.Active, capacity });
	    }

	    //
	    // Public
	    //
		public ModComponentSystem CreateManager(Type type, params object[] constructorArgumnents)
		{
			return CreateManagerInternal(type, GetCapacityForType(type), constructorArgumnents);
		}

		public T CreateManager<T>(params object[] constructorArgumnents) where T : ModComponentSystem
		{
			return (T)CreateManagerInternal(typeof(T), GetCapacityForType(typeof(T)), constructorArgumnents);
		}

		public T GetOrCreateManager<T> () where T : ModComponentSystem
		{
			return (T)GetOrCreateManagerInternal (typeof(T));
		}

		public ModComponentSystem GetOrCreateManager(Type type)
		{
			return GetOrCreateManagerInternal (type);
		}

		public T GetExistingManager<T> () where T : ModComponentSystem
		{
			return (T)GetExistingManagerInternal (typeof(T));
		}

		public ModComponentSystem GetExistingManager(Type type)
		{
			return GetExistingManagerInternal (type);
		}

		public void DestroyManager(ModComponentSystem manager)
		{
			RemoveManagerInteral(manager);
			RemoveManagerExtraInternal(manager);
		}
    }
}
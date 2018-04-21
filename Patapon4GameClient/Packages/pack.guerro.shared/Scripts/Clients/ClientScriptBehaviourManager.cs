using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using JetBrains.Annotations;
using Packet.Guerro.Shared.Clients;
using UnityEngine;
using Unity.Entities;

namespace Packages.pack.guerro.shared.Scripts.Clients
{
    public abstract class ClientScriptBehaviourManager : ComponentSystem
    {
	    public ClientWorld ClientWorld;
	    
        [Inject] protected SharedClientGroup ClientGroup;

        internal void AddManagerForClient(ClientWorld clientWorld)
        {
	        ClientWorld = clientWorld;
        }
    }

    public class ClientWorld
    {
        // Well, it somewhat copy the same code of Entity/World.cs
	    public ClientEntity Client   { get; private set; }
	    public int          ClientId { get; private set; }
	    
	    [NotNull]
	    private static MethodInfo s_DestroyInstance;

	    [NotNull]
	    private static MethodInfo s_CreateInstance;
	    
        public IEnumerable<ClientScriptBehaviourManager> BehaviourManagers =>
            new ReadOnlyCollection<ClientScriptBehaviourManager>(m_BehaviourManagers);

        private List<ClientScriptBehaviourManager> m_BehaviourManagers = new List<ClientScriptBehaviourManager>();

        //@TODO: What about multiple managers of the same type...
        Dictionary<Type, ClientScriptBehaviourManager> m_BehaviourManagerLookup =
            new Dictionary<Type, ClientScriptBehaviourManager>();
	    
	    static Dictionary<ClientEntity, ClientWorld> s_WorldsClientIdLookup =
		    new Dictionary<ClientEntity, ClientWorld>();

        int m_DefaultCapacity = 10;
        bool m_AllowGetManager = true;

        public int Version { get { return m_Version; } }
        int m_Version = 0;
	    
	    internal static readonly List<ClientWorld> AllWorlds = new List<ClientWorld>();

        int GetCapacityForType(Type type)
        {
            return m_DefaultCapacity;
        }

        public void SetDefaultCapacity(int value)
        {
            m_DefaultCapacity = value;
        }

	    static ClientWorld()
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

	    public static ClientWorld GetOrCreate(ClientEntity clientEntity)
	    {
		    if (s_WorldsClientIdLookup.TryGetValue(clientEntity, out var world))
		    {
			    return world;
		    }

		    return new ClientWorld(clientEntity);
	    }

        public ClientWorld(ClientEntity clientEntity)
        {
	        ClientId = (Client = clientEntity).ReferenceId;

	        s_WorldsClientIdLookup[clientEntity] = this;
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
	    ClientScriptBehaviourManager CreateManagerInternal (Type type, int capacity, object[] constructorArguments)
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
		    ClientScriptBehaviourManager manager;
		    try
		    {
		        manager = Activator.CreateInstance(type, constructorArguments) as ClientScriptBehaviourManager;

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
			    manager.AddManagerForClient(this);
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

	    ClientScriptBehaviourManager GetExistingManagerInternal (Type type)
		{
#if ENABLE_UNITY_COLLECTIONS_CHECKS
		    if (!IsCreated)
		        throw new ArgumentException("During destruction ");
			if (!m_AllowGetManager)
				throw new ArgumentException("During destruction of a system you are not allowed to get or create more systems.");
#endif

			ClientScriptBehaviourManager manager ;
			if (m_BehaviourManagerLookup.TryGetValue(type, out manager))
				return manager;

			return null;
		}

	    ClientScriptBehaviourManager GetOrCreateManagerInternal (Type type)
		{
			var manager = GetExistingManagerInternal(type);

			return manager ?? CreateManagerInternal(type, GetCapacityForType(type), null);
		}

	    void AddTypeLookup(Type type, ClientScriptBehaviourManager manager)
	    {
	        while (type != typeof(ScriptBehaviourManager))
	        {
	            if (!m_BehaviourManagerLookup.ContainsKey(type))
	                m_BehaviourManagerLookup.Add(type, manager);

	            type = type.BaseType;
	        }
	    }

	    void RemoveManagerInteral(ClientScriptBehaviourManager manager)
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
	    void RemoveManagerExtraInternal(ClientScriptBehaviourManager manager)
	    {
		    s_DestroyInstance.Invoke(manager, null);
	    }
	    
	    void CreateManagerExtraInternal(ClientScriptBehaviourManager manager, int capacity)
	    {
		    s_CreateInstance.Invoke(manager, new object[]{ World.Active, capacity });
	    }

	    //
	    // Public
	    //
		public ClientScriptBehaviourManager CreateManager(Type type, params object[] constructorArgumnents)
		{
			return CreateManagerInternal(type, GetCapacityForType(type), constructorArgumnents);
		}

		public T CreateManager<T>(params object[] constructorArgumnents) where T : ClientScriptBehaviourManager
		{
			return (T)CreateManagerInternal(typeof(T), GetCapacityForType(typeof(T)), constructorArgumnents);
		}

		public T GetOrCreateManager<T> () where T : ClientScriptBehaviourManager
		{
			return (T)GetOrCreateManagerInternal (typeof(T));
		}

		public ClientScriptBehaviourManager GetOrCreateManager(Type type)
		{
			return GetOrCreateManagerInternal (type);
		}

		public T GetExistingManager<T> () where T : ClientScriptBehaviourManager
		{
			return (T)GetExistingManagerInternal (typeof(T));
		}

		public ClientScriptBehaviourManager GetExistingManager(Type type)
		{
			return GetExistingManagerInternal (type);
		}

		public void DestroyManager(ClientScriptBehaviourManager manager)
		{
			RemoveManagerInteral(manager);
			RemoveManagerExtraInternal(manager);
		}
    }
}
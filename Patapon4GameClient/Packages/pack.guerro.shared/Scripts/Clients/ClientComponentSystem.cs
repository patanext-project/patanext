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
    public abstract class ClientComponentSystem : ComponentSystem
    {
	    public ClientWorld ClientWorld;
	    
        [Inject] protected SharedClientGroup ClientGroup;

        internal void AddManagerForClient(ClientWorld clientWorld)
        {
	        ClientWorld = clientWorld;
        }
    }

	public abstract class ClientDataContainer
	{
		public ClientEntity Client { get; private set; }
		public ClientWorld World { get; private set; }

		protected virtual void OnCreateContainer()
		{
		}

		protected virtual void OnDestroyContainer()
		{
		}

		internal void CreateInternal()
		{
			OnCreateContainer();
		}

		internal void DestroyInternal()
		{
			OnDestroyContainer();
		}

		internal void AddContainerForClient(ClientEntity clientEntity, ClientWorld world)
		{
			Client = clientEntity;
			World = world;
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
	    
        public IEnumerable<ClientComponentSystem> BehaviourManagers =>
            new ReadOnlyCollection<ClientComponentSystem>(m_BehaviourManagers);

        private List<ClientComponentSystem> m_BehaviourManagers = new List<ClientComponentSystem>();

	    private FastDictionary<Type, ClientDataContainer> m_ClientDataContainers =
		    new FastDictionary<Type, ClientDataContainer>();

        //@TODO: What about multiple managers of the same type...
	    FastDictionary<Type, ClientComponentSystem> m_BehaviourManagerLookup =
            new FastDictionary<Type, ClientComponentSystem>();
	    
	    static FastDictionary<ClientEntity, ClientWorld> s_WorldsClientIdLookup =
		    new FastDictionary<ClientEntity, ClientWorld>();

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

	    #region Managers Creation/Destruction
	    //
	    // Internal
	    //
	    ClientComponentSystem CreateManagerInternal (Type type, int capacity, object[] constructorArguments)
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
		    ClientComponentSystem manager;
		    try
		    {
		        manager = Activator.CreateInstance(type, constructorArguments) as ClientComponentSystem;

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

	    ClientComponentSystem GetExistingManagerInternal (Type type)
		{
#if ENABLE_UNITY_COLLECTIONS_CHECKS
		    if (!IsCreated)
		        throw new ArgumentException("During destruction ");
			if (!m_AllowGetManager)
				throw new ArgumentException("During destruction of a system you are not allowed to get or create more systems.");
#endif

			ClientComponentSystem manager ;
			if (m_BehaviourManagerLookup.TryGetValue(type, out manager))
				return manager;

			return null;
		}

	    ClientComponentSystem GetOrCreateManagerInternal (Type type)
		{
			var manager = GetExistingManagerInternal(type);

			return manager ?? CreateManagerInternal(type, GetCapacityForType(type), null);
		}

	    void AddTypeLookup(Type type, ClientComponentSystem manager)
	    {
	        while (type != typeof(ScriptBehaviourManager))
	        {
	            if (!m_BehaviourManagerLookup.ContainsKey(type))
	                m_BehaviourManagerLookup.Add(type, manager);

	            type = type.BaseType;
	        }
	    }

	    void RemoveManagerInteral(ClientComponentSystem manager)
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
	    void RemoveManagerExtraInternal(ClientComponentSystem manager)
	    {
		    s_DestroyInstance.Invoke(manager, null);
	    }
	    
	    void CreateManagerExtraInternal(ClientComponentSystem manager, int capacity)
	    {
		    s_CreateInstance.Invoke(manager, new object[]{ World.Active, capacity });
	    }

	    //
	    // Public
	    //
		public ClientComponentSystem CreateManager(Type type, params object[] constructorArgumnents)
		{
			return CreateManagerInternal(type, GetCapacityForType(type), constructorArgumnents);
		}

		public T CreateManager<T>(params object[] constructorArgumnents) where T : ClientComponentSystem
		{
			return (T)CreateManagerInternal(typeof(T), GetCapacityForType(typeof(T)), constructorArgumnents);
		}

		public T GetOrCreateManager<T> () where T : ClientComponentSystem
		{
			return (T)GetOrCreateManagerInternal (typeof(T));
		}

		public ClientComponentSystem GetOrCreateManager(Type type)
		{
			return GetOrCreateManagerInternal (type);
		}

		public T GetExistingManager<T> () where T : ClientComponentSystem
		{
			return (T)GetExistingManagerInternal (typeof(T));
		}

		public ClientComponentSystem GetExistingManager(Type type)
		{
			return GetExistingManagerInternal (type);
		}

		public void DestroyManager(ClientComponentSystem manager)
		{
			RemoveManagerInteral(manager);
			RemoveManagerExtraInternal(manager);
		}
	    #endregion
	   
#region Containers Creation/Destruction
	    //
	    // Internal
	    //
	    ClientDataContainer CreateContainerInternal (Type type, int capacity, object[] constructorArguments)
		{
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			if (constructorArguments != null && constructorArguments.Length != 0)
			{
				var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				if (constructors.Length == 1 && constructors[0].IsPrivate)
					throw new MissingMethodException($"Constructing {type} failed because the constructor was private, it must be public.");
			}
#endif

		    ClientDataContainer container;
		    try
		    {
			    container = Activator.CreateInstance(type, constructorArguments) as ClientDataContainer;

		    }
		    catch
		    {
		        throw;
		    }

			m_ClientDataContainers[type] = container;

		    try
		    {
			    container.AddContainerForClient(Client, this);
		        CreateContainerExtraInternal(container);

		    }
		    catch
		    {
		        RemoveContainerInteral(container);
		        throw;
		    }

		    ++m_Version;
			return container;
		}

	    ClientDataContainer GetExistingContainerInternal (Type type)
		{
#if ENABLE_UNITY_COLLECTIONS_CHECKS
		    if (!IsCreated)
		        throw new ArgumentException("During destruction ");
#endif

			ClientDataContainer Container ;
			if (m_ClientDataContainers.TryGetValue(type, out Container))
				return Container;

			return null;
		}

	    ClientDataContainer GetOrCreateContainerInternal (Type type)
		{
			var Container = GetExistingContainerInternal(type);

			return Container ?? CreateContainerInternal(type, GetCapacityForType(type), null);
		}

	    void RemoveContainerInteral(ClientDataContainer container)
		{
			var type = container.GetType();
			if (!m_ClientDataContainers.Remove(type))
				throw new ArgumentException($"Container does not exist in the world");
		    ++m_Version;

			m_ClientDataContainers.Remove(type);
		}

	    //
	    // Extra internal
	    //
	    void RemoveContainerExtraInternal(ClientDataContainer container)
	    {
		    container.DestroyInternal();
	    }
	    
	    void CreateContainerExtraInternal(ClientDataContainer container)
	    {
		    container.CreateInternal();
	    }

	    //
	    // Public
	    //
		public ClientDataContainer CreateContainer(Type type, params object[] constructorArgumnents)
		{
			return CreateContainerInternal(type, GetCapacityForType(type), constructorArgumnents);
		}

		public T CreateContainer<T>(params object[] constructorArgumnents) where T : ClientDataContainer
		{
			return (T)CreateContainerInternal(typeof(T), GetCapacityForType(typeof(T)), constructorArgumnents);
		}

		public T GetOrCreateContainer<T> () where T : ClientDataContainer
		{
			return (T)GetOrCreateContainerInternal (typeof(T));
		}

		public ClientDataContainer GetOrCreateContainer(Type type)
		{
			return GetOrCreateContainerInternal (type);
		}

		public T GetExistingContainer<T> () where T : ClientDataContainer
		{
			return (T)GetExistingContainerInternal (typeof(T));
		}

		public ClientDataContainer GetExistingContainer(Type type)
		{
			return GetExistingContainerInternal (type);
		}

		public void DestroyContainer(ClientDataContainer Container)
		{
			RemoveContainerInteral(Container);
			RemoveContainerExtraInternal(Container);
		}

	    public T SetContainer<T>(T container)
	    	where T : ClientDataContainer
	    {
		    if (m_ClientDataContainers.ContainsKey(typeof(T)))
		    {
			    if (m_ClientDataContainers[typeof(T)] != container)
			    {
				    container.AddContainerForClient(Client, this);
				    container.CreateInternal();
			    }
		    }
		    else
		    {
			    container.AddContainerForClient(Client, this);
			    container.CreateInternal();
		    }
		    
		    m_ClientDataContainers[typeof(T)] = container;
		    return container;
	    }
	    #endregion
    }
}
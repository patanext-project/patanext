using System;
using System.Collections.Generic;
using Guerro.Utilities;
using Packages.pack.guerro.shared.Scripts.Clients;
using Packet.Guerro.Shared.Game;
using Packet.Guerro.Shared.Game.Behaviours;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Experimental.Input;

namespace Packet.Guerro.Shared.Clients
{
    public class ClientManager : ComponentSystem
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Structs
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public struct SearchFilter
        {
            public int FilterMainClient;
            public int FilterLivableClients;
        }

        [Inject] private SharedClientGroup m_ClientGroup;
        
        protected override void OnUpdate()
        {
            if (m_ClientGroup.Length > 1 && !EnableMultiClient)
                throw new Exception("There is more than one client.");
        }
        
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        
        /// <summary>
        /// Only available if 'EnableMultiClient' is set to false
        /// </summary>
        public static ClientEntity MainClient;

        private static bool s_EnableMultiClient;
        public static bool EnableMultiClient
        {
            get => s_EnableMultiClient;
            set
            {
                // todo: add events
                s_EnableMultiClient = value;
            }
        }
        
        public int Count { get; private set; }
        
        public delegate void OnNewClientEvent(ClientEntity clientId);
        public static event OnNewClientEvent OnNewClient;
        
        // internal
        private FastDictionary<int, ClientEntity> m_AllClients;
        private NativeList<ClientEntity> m_AllLivingClients;
        private NativeQueue<ClientEntity> m_PooledClients;
        private int m_MaxId;

        [Inject] private CGameEntityGroupManager m_GroupManager;
        
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        protected override void OnCreateManager(int capacity)
        {
            base.OnCreateManager(capacity);
            
            m_AllClients = new FastDictionary<int, ClientEntity>();
            m_AllLivingClients = new NativeList<ClientEntity>(Allocator.Persistent);
            m_PooledClients = new NativeQueue<ClientEntity>(Allocator.Persistent);
        }

        protected override void OnDestroyManager()
        {
            base.OnDestroyManager();

            OnNewClient = null;
            
            m_AllClients.Clear();
            m_PooledClients.Dispose();
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Creation
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public ClientEntity Connect(string login, string password)
        {
            if (!EnableMultiClient && m_AllClients.Count > 0)
                throw new Exception("Too much clients");
            
            // todo: implement connect
            var clientId = Create(login);

            OnNewClient?.Invoke(clientId);
            
            return clientId;
        }

        public ClientEntity Create(string userLogin)
        {
            if (!EnableMultiClient && m_AllClients.Count > 0)
                throw new Exception("Too much clients");
            
            var clientId = CreateInternal();

            OnNewClient?.Invoke(clientId);
            
            return clientId;
        }
        
        // internal
        private ClientEntity CreateInternal()
        {            
            // get from pool
            if (!m_PooledClients.TryDequeue(out var clientEntity))
            {
                var entity = EntityManager.CreateEntity(typeof(ClientEntity), typeof(EntityGroup));
                var group = m_GroupManager.CreateGroup();
                
                m_GroupManager.AttachTo(entity, group);
                
                clientEntity.ReferenceId = m_MaxId++;
                clientEntity.CachedEntity = entity;
            }

            clientEntity.IsCreated = true;
            
            clientEntity.CachedEntity.SetComponentData(clientEntity);

            ClientWorld.GetOrCreate(clientEntity);

            if (!EnableMultiClient)
            {
                MainClient = clientEntity;
            }

            m_AllClients[MainClient.ReferenceId] = clientEntity;
            m_AllLivingClients.Add(clientEntity);
            
            return clientEntity;
        }
        
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Retrieve
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public ClientEntity Get(int index)
        {
            throw new NotImplementedException();
        }

        public NativeArray<ClientEntity> GetClients(SearchFilter filter, Allocator allocator)
        {
            if (filter.FilterLivableClients == 1
                && filter.FilterMainClient != 0)
            {
                return new NativeArray<ClientEntity>(m_AllLivingClients.ToDeferredJobArray(), allocator);
            }
            
            return new NativeArray<ClientEntity>(0, Allocator.Invalid);
        }

        public ClientWorld GetWorld(ClientEntity clientEntity)
        {
            return ClientWorld.GetOrCreate(clientEntity);
        }
        
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Check
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public bool Exists(ClientEntity clientEntity)
        {
            return Exists(clientEntity.ReferenceId);
        }

        public bool Exists(int id)
        {
            for (int i = 0; i != m_AllLivingClients.Length; i++)
            {
                if (m_AllLivingClients[i].ReferenceId == id)
                    return true;
            }

            return false;
        }
        
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Reset
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public void ResetByIndex(int index)
        {
            throw new NotImplementedException();
        }

        public void ResetById(int index)
        {
            throw new NotImplementedException();
        }
    }
}
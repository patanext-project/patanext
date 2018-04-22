using System;
using System.Collections.Generic;
using Packages.pack.guerro.shared.Scripts.Clients;
using Packet.Guerro.Shared.Game;
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
        
        protected override void OnUpdate()
        {
            
        }
        
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public int Count { get; private set; }
        
        public delegate void OnNewClientEvent(ClientEntity clientId);
        public static event OnNewClientEvent OnNewClient;
        
        // internal
        private FastDictionary<int, ClientEntity> m_AllClients;
        private NativeList<ClientEntity> m_AllLivingClients;
        private NativeQueue<ClientEntity> m_PooledClients;
        private int m_MaxId;
        
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        protected override void OnCreateManager(int capacity)
        {
            base.OnCreateManager(capacity);
            
            m_AllClients = new FastDictionary<int, ClientEntity>();
            m_AllLivingClients = new NativeList<ClientEntity>();
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
            // todo: implement connect
            var clientId = Create(login);

            OnNewClient?.Invoke(clientId);
            
            return clientId;
        }

        public ClientEntity Create(string userLogin)
        {
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
                clientEntity.ReferenceId = m_MaxId++;
            }

            clientEntity.IsCreated = true;

            new ClientWorld(clientEntity);
            
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
            throw new NotImplementedException();
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
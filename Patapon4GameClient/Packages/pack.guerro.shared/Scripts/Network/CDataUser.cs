using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Packet.Guerro.Shared.Network
{
    public partial struct CDataUser
    {
        private Entity m_Entity;
        private int m_EntityId;
        public int ReferenceId;

        public string Login;

        private CDataUser(int referenceId)
        {
            this.ReferenceId = referenceId;
            Login = string.Empty;
            
            m_Entity = Entity.Null;
            m_EntityId = -1;
        }

        public Entity ToEntity(World world)
        {
            var entityManager = world.GetExistingManager<EntityManager>();
            var entities = entityManager.GetAllEntities();
            if (m_EntityId != -1)
            {
                if (m_Entity.Index == m_EntityId)
                    return m_Entity;
                Debug.LogWarning("IDs of Entities don't match!");
            }

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                if (entityManager.HasComponent<UserEntity>(entity)
                && entityManager.GetComponentData<UserEntity>(entity).ReferenceId == ReferenceId)
                {
                    Debug.LogError("The user should have cached the reference to the entity.");
                    m_EntityId = entity.Index;
                    m_Entity = entity;
                    return entity;
                }
            }

            var newEntity = entityManager.CreateEntity(typeof(UserEntity));
            entityManager.SetComponentData(newEntity, new UserEntity() { ReferenceId = ReferenceId });
            m_EntityId = newEntity.Index;
            m_Entity = newEntity;
            return newEntity;
        }

        public void AddModule<T>(World world, T module)
            where T : struct, IComponentData
        {
            var entity = ToEntity(world);
            var entityManager = world.GetExistingManager<EntityManager>();
            entityManager.AddComponentData(entity, module);
        }
    }

    public partial struct CDataUser
    {
        private static int s_referenceIdCount;
        private static FastDictionary<int, CDataUser> m_List = new FastDictionary<int, CDataUser>();

        static CDataUser()
        {
            Application.quitting += () => { m_List.Clear(); m_List = null; };
        }

        public static CDataUser Get(int referenceId)
        {
            CDataUser toReturn;
            if (!m_List.FastTryGet(referenceId, out toReturn))
            {
                Debug.LogError("no user found!");
            }
            return toReturn;
        }

        public static CDataUser Create()
        {
            var user = new CDataUser(s_referenceIdCount);
            m_List.Add(s_referenceIdCount, user);

            s_referenceIdCount++;

            return user;
        }

        public static CDataUser Create(CDataUser copy)
        {
            var user = copy;
            user.m_Entity = Entity.Null;
            user.m_EntityId = -1;
            
            user.ReferenceId = s_referenceIdCount;
            m_List.Add(s_referenceIdCount, user);

            s_referenceIdCount++;

            return user;
        }

        public static void Update(CDataUser user)
        {
            m_List[user.ReferenceId] = user;
        }
    }
}
using System;
using System.Collections.Generic;
using Guerro.Utilities;
using Packet.Guerro.Shared.Game.Behaviours;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Packet.Guerro.Shared.Game
{
    public class CGameEntityGroupManager : ComponentSystem
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Groups
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        struct Group
        {
            public ComponentDataArray<EntityGroup> EntityGroups;
            public int Length;
        }
        
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        [Inject] private Group m_EntityGroup;
        private EntityArchetype m_Archetype;
        private FastDictionary<int, int> m_ApplyGroupIdFromSceneGroupIds;
        private FastDictionary<int, EntityGroup> m_Groups;
        private NativeQueue<EntityGroup> m_PooledGroups;

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Base Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        protected override void OnCreateManager(int capacity)
        {
            m_Archetype = EntityManager.CreateArchetype(typeof(EntityGroup));
            m_ApplyGroupIdFromSceneGroupIds = new FastDictionary<int, int>();
            m_Groups = new FastDictionary<int, EntityGroup>();
            m_PooledGroups = new NativeQueue<EntityGroup>(Allocator.Persistent);
        }

        protected override void OnDestroyManager()
        {
            m_ApplyGroupIdFromSceneGroupIds.Clear();
            m_Groups.Clear();
            m_PooledGroups.Clear();

            m_ApplyGroupIdFromSceneGroupIds = null;
            m_Groups = null;
        }
        
        protected override void OnUpdate()
        {
            // Remove automatically groups with 0 entities in it
            // If none are found, we remove the group from the list.
            // If an entity is attached to the group with the same id, we use another version of this id (a là Entity class)
            var listEventKeepIds = new NativeList<bool1>(m_EntityGroup.Length, Allocator.Temp);
            for (int i = 0; i != m_EntityGroup.Length; i++)
            {
                var group = m_EntityGroup.EntityGroups[i];
                listEventKeepIds[group.ReferenceId] = true;
            }

            for (int i = 0; i != m_EntityGroup.Length; i++)
            {
                if (!listEventKeepIds[i])
                {
                    var group = m_Groups[i];
                    
                    Debug.Log($"Group({i}, {group.Version}) was unused, so it was removed.");

                    group.IsCreated = false;
                    group.Version++;
                    
                    m_PooledGroups.Enqueue(group);
                    m_Groups.Remove(i);
                }
            }
            
            listEventKeepIds.Dispose();
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public EntityGroup Create()
        {
            EntityGroup componentValue;
            // If we have some data left in the queue, dequeue it.
            if (m_PooledGroups.Count > 0)
                componentValue = m_PooledGroups.Dequeue();
            else
                componentValue = new EntityGroup
                {
                    ReferenceId = m_Groups.Count
                };

            componentValue.IsCreated = true;

            m_Groups[componentValue.ReferenceId] = componentValue;

            return componentValue;
        }

        public void AttachTo(Entity entity, EntityGroup entityGroup)
        {
            if (!entityGroup.IsCreated)
                throw new Exception($"The group({entityGroup.ReferenceId}, {entityGroup.Version}) doesn't exist anymore.");
            
            entity.SetOrCreateComponentData(entityGroup);
        }

        public void AttachTo(Entity entity, int entityGroupId)
        {
            var entityGroup = m_Groups[entityGroupId];
            
            entity.SetOrCreateComponentData(entityGroup);
        }

        internal EntityGroup AttachBehaviour(EntityGroupBehaviour behaviour)
        {
            if (!m_ApplyGroupIdFromSceneGroupIds.FastTryGet(behaviour.SceneGroupId, out var groupId))
            {
                var data = Create();

                m_ApplyGroupIdFromSceneGroupIds[behaviour.SceneGroupId] = data.ReferenceId;

                return data;
            }

            return m_Groups[groupId];
        }
    }
}
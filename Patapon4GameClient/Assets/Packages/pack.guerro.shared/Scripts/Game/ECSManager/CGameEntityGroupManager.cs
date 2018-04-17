using System;
using System.Collections.Generic;
using Guerro.Utilities;
using JetBrains.Annotations;
using Packet.Guerro.Shared.Game.Behaviours;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Packet.Guerro.Shared.Game
{
    [UsedImplicitly]
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

        private FastDictionary<int, bool> m_CachedDictionaryEventKeepIds;
        private int m_MaxId;
        
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Base Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        protected override void OnCreateManager(int capacity)
        {
            m_Archetype = EntityManager.CreateArchetype(typeof(EntityGroup));
            m_ApplyGroupIdFromSceneGroupIds = new FastDictionary<int, int>();
            m_Groups = new FastDictionary<int, EntityGroup>();
            m_PooledGroups = new NativeQueue<EntityGroup>(Allocator.Persistent);
            
            m_CachedDictionaryEventKeepIds = new FastDictionary<int, bool>();
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
            var groupLength = m_Groups.Count;
            
            m_CachedDictionaryEventKeepIds.Clear();
            
            // Remove automatically groups with 0 entities in it
            // If none are found, we remove the group from the list.
            // If an entity is attached to the group with the same id, we use another version of this id (a là Entity class)
            for (int i = 0; i != m_EntityGroup.Length; i++)
            {
                var group = m_EntityGroup.EntityGroups[i];
                m_CachedDictionaryEventKeepIds[group.ReferenceId] = true;
            }
            for (int i = 0; i != m_MaxId; i++)
            {
                if (m_Groups.FastTryGet(i, out var _)
                    && !m_CachedDictionaryEventKeepIds.FastTryGet(i, out var __))
                {
                    var group = m_Groups[i];
                    
                    Debug.Log($"Group({i}, {group.Version}) was unused, so it was removed.");

                    group.IsCreated = false;
                    group.Version++;
                    
                    m_PooledGroups.Enqueue(group);
                    m_Groups.Remove(i);
                }
            }
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public EntityGroup CreateGroup()
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

            if (m_Groups.Count > m_MaxId)
            {
                m_MaxId = m_Groups.Count;
            }

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
            if (behaviour.SceneGroupId < 0)
            {
                return CreateGroup();
            }
            
            if (!m_ApplyGroupIdFromSceneGroupIds.FastTryGet(behaviour.SceneGroupId, out var groupId))
            {
                var data = CreateGroup();

                m_ApplyGroupIdFromSceneGroupIds[behaviour.SceneGroupId] = data.ReferenceId;

                return data;
            }

            return m_Groups[groupId];
        }
    }
}
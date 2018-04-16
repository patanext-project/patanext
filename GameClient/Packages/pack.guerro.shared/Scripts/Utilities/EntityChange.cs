using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Guerro.Utilities
{
    /*public struct EntityComponentChange
    {
        public NativeList<IComponentData> m_ChangedComponents;
        public int Length { get { return m_ChangedComponents.Length; }}
        public IComponentData this[int index]
        {
            get { return m_ChangedComponents[index]; }
        }
        
    }*/

    public struct EntityChangeItem
    {
        public Entity entity;

        public bool1 wasCreated;
        public bool1 wasModified;
    }
    
    public struct EntityChange
    {
        private FastDictionary<int, Entity> m_CurrentEntities;
        private NativeList<EntityChangeItem> m_Entities;
        private int m_LastLength;

        public int EntitySpawnCount;
        public int EntityReplaceCount;
        public int EntityRemovalCount;

        public bool NeedEntitiesList,
            NeedToCheckReplacement,
            NeedToCheckRemoval,
            NeedToAddEntities;

        public int Length { get { return m_Entities.Length; }}
        public EntityChangeItem this[int index]
        {
            get { return m_Entities[index]; }
        }

        public EntityChange(int capacity)
        {
            m_CurrentEntities   = new FastDictionary<int, Entity>(capacity);
            m_Entities          = new NativeList<EntityChangeItem>(capacity, Allocator.Persistent);
            EntitySpawnCount    = 0;
            EntityReplaceCount  = 0;
            EntityRemovalCount  = 0;
            m_LastLength = 0;
            NeedEntitiesList = true;
            NeedToCheckReplacement = true;
            NeedToCheckRemoval = true;
            NeedToAddEntities = true;
        }

        public void Update(ref EntityArray array)
        {
            m_Entities.Clear();
            EntitySpawnCount    = 0;
            EntityReplaceCount   = 0;
            EntityRemovalCount  = 0;
  
            var defaultEntity = new Entity();
            var length = array.Length;
            if (NeedEntitiesList)
            {
                UnityEngine.Profiling.Profiler.BeginSample("ForLoop #1 - With entities");
                for (int i = 0; i != length; i++)
                {
                    Entity entity;
                    UnityEngine.Profiling.Profiler.BeginSample("Get entities and init var");
                    var checkedEntity = array[i];
                    UnityEngine.Profiling.Profiler.EndSample();
                    UnityEngine.Profiling.Profiler.BeginSample("TryGet()");
                    if (!m_CurrentEntities.FastTryGet(checkedEntity.Index, out entity))
                    {
                        if (NeedEntitiesList)
                        {
                            m_Entities.Add(new EntityChangeItem() {
                                entity = checkedEntity,
                                wasCreated = true
                            });
                        }
                        m_CurrentEntities[checkedEntity.Index] =  checkedEntity;
                        EntitySpawnCount++;
                    }
                    else if (NeedToCheckReplacement)
                    {
                        UnityEngine.Profiling.Profiler.BeginSample("Check Version");
                        // The entity was replaced
                        if (entity.Version != checkedEntity.Version)
                        {
                            EntityReplaceCount++;
                            m_CurrentEntities[checkedEntity.Index] = checkedEntity;
                        } 
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                    UnityEngine.Profiling.Profiler.EndSample();
                }
                UnityEngine.Profiling.Profiler.EndSample();
            }
            else
            {
                UnityEngine.Profiling.Profiler.BeginSample("ForLoop #1 - Without entities");
                EntitySpawnCount = length - m_LastLength;
                m_LastLength = length;
                UnityEngine.Profiling.Profiler.EndSample();
            }

            UnityEngine.Profiling.Profiler.BeginSample("Check for removal");
            if (NeedToCheckRemoval && array.Length != m_CurrentEntities.Count)
            {
                var toRemove = new NativeList<int>(Allocator.Temp);
                foreach (var entity in m_CurrentEntities.Values)
                {
                    var exist = false;
                    for (int i = 0; i != array.Length; i++)
                    {
                        if (array[i].Index == entity.Index)
                        {
                            exist = true; 
                            break;
                        }
                    }
                    if (exist)
                        continue;
                    EntityRemovalCount++;
                    m_Entities.Add(new EntityChangeItem()
                    {
                        entity = entity,
                        wasCreated = false
                    });

                    toRemove.Add(entity.Index);
                }
                for (int i = 0; i != toRemove.Length; i++)
                    m_CurrentEntities.Remove(toRemove[i]);

                toRemove.Dispose();
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}
using System;
using System.Reflection;
using Guerro.Utilities;
using Unity.Entities;
using UnityEngine;

namespace Packet.Guerro.Shared.Game
{
    public abstract class CGameEntityCreatorBehaviour<TSystem> : MonoBehaviour
        where TSystem : CGameEntityCreatorSystem
    {
        private GameObjectEntity m_GameObjectEntity;

        private void Awake()
        {
            m_GameObjectEntity = GetComponent<GameObjectEntity>()
                                 ?? gameObject.AddComponent<GameObjectEntity>();

            AwakeBeforeFilling();
            FillEntityData();
            AwakeAfterFilling();
            
        }

        protected virtual void AwakeBeforeFilling()
        {
        }
        protected virtual void AwakeAfterFilling()
        {
        }

        public virtual void FillEntityData()
        {
            var world  = World.Active;
            var system = world.GetOrCreateManager<TSystem>();
            system.FillEntityData(gameObject, m_GameObjectEntity.Entity);

            // todo: find a better way to update GameObjectEntity without disabling it? (this is really a bad way to do that)
            var components = gameObject.GetComponents<ComponentDataWrapperBase>();
            var parameters = new object[] {m_GameObjectEntity.EntityManager, m_GameObjectEntity.Entity};
            for (int i = 0; i != components.Length; ++i)
            {
                var comp = components[i];
                // We need to use reflection to sync the entity components :(
                // ReSharper disable PossibleNullReferenceException
                var type = (ComponentType) comp.GetType()
                                               .GetMethod("GetComponentType", BindingFlags.NonPublic
                                                                              | BindingFlags.Instance)
                                               .Invoke(comp, null);

                if (!m_GameObjectEntity.EntityManager.HasComponent(m_GameObjectEntity.Entity, type))
                    m_GameObjectEntity.EntityManager.AddComponent(m_GameObjectEntity.Entity, type);

                comp.GetType()
                    .GetMethod("UpdateComponentData", BindingFlags.NonPublic
                                                      | BindingFlags.Instance)
                    .Invoke(comp, parameters);
                // ReSharper restore PossibleNullReferenceException
            }
        }

        private void OnDestroy()
        {
            // Remove all references
            m_GameObjectEntity = null;
        }
    }

    public abstract class CGameEntityCreatorSystem : ComponentSystem
    {
        public abstract void FillEntityData(GameObject gameObject, Entity entity);

        protected void AddComponents(GameObject gameObject, params Type[] types)
        {
            for (int i = 0; i != types.Length; ++i)
            {
                if (gameObject.GetComponent(types[i]) == null)
                    gameObject.AddComponent(types[i]);
            }
        }

        protected void AddComponentData<T>(Entity entity, T value)
            where T : struct, IComponentData
        {
            entity.SetOrCreateComponentData(value, World);
        }

        protected void AddFixedArray<T>(Entity entity, int length)
            where T : struct 
        {
            var typeIndex = TypeManager.GetTypeIndex<T>();

            void LocalCreateFixedArray()
            {
                EntityManager.AddComponent(entity, ComponentType.FixedArray(typeof(T), length));
            }
            
            if (EntityManager.HasComponent(entity, ComponentType.FromTypeIndex(typeIndex)))
            {
                var fixedArray = EntityManager.GetFixedArray<T>(entity);
                if (fixedArray.Length != length)
                {
                    EntityManager.RemoveComponent(entity, ComponentType.FromTypeIndex(typeIndex));
                    LocalCreateFixedArray();
                }
            }
            else
                LocalCreateFixedArray();
        }
    }
}
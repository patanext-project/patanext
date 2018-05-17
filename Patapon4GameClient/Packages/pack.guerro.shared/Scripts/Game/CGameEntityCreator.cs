using System;
using System.Reflection;
using Guerro.Utilities;
using Packages.pack.guerro.shared.Scripts.Utilities;
using Unity.Entities;
using UnityEngine;

namespace Packet.Guerro.Shared.Game
{
    public abstract class CGameEntityCreatorBehaviour<TSystem> : MonoBehaviour
        where TSystem : CGameEntityCreatorSystem
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        private GameObjectEntity m_GameObjectEntity;

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Unity Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        private void Awake()
        {
            m_GameObjectEntity = GetComponent<GameObjectEntity>()
                                 ?? gameObject.AddComponent<GameObjectEntity>();

            AwakeBeforeFilling();
            FillEntityData();
            AwakeAfterFilling();
            
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // to be filled by childs classes
        protected virtual void AwakeBeforeFilling()
        {
        }
        // to be filled by childs classes
        protected virtual void AwakeAfterFilling()
        {
        }

        // todo: write how it work, and what it does
        public virtual void FillEntityData()
        {
            var world  = World.Active;
            var system = world.GetOrCreateManager<TSystem>();
            system.FillEntityData(gameObject, m_GameObjectEntity.Entity);

            /*// todo: find a better way to update GameObjectEntity without disabling it? (this is really a bad way to do that)
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

            var components = gameObject.GetComponents<Component>();
            for (int i = 0; i != components.Length; ++i)
            {
                var compWrapper = components[i] as ComponentDataWrapperBase;
                if (compWrapper != null)
                {
                    
                }
                else
                {
                    
                }
            }*/
            gameObject.CustomRefresh();;
        }

        private void OnDestroy()
        {
            // Remove all references
            m_GameObjectEntity = null;
        }
    }

    public abstract class CGameEntityCreatorSystem : ComponentSystem
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public abstract void FillEntityData(GameObject gameObject, Entity entity);

        /// <summary>
        /// Add an unity component to the entity and gameObject
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="types"></param>
        protected void AddComponents(GameObject gameObject, params Type[] types)
        {
            for (int i = 0; i != types.Length; ++i)
            {
                if (gameObject.GetComponent(types[i]) == null)
                    gameObject.AddComponent(types[i]);
            }
        }

        /// <summary>
        /// Add a component to the entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        protected void AddComponentData<T>(Entity entity, T value)
            where T : struct, IComponentData
        {
            entity.SetOrCreateComponentData(value, World);
        }
        
        /// <summary>
        /// Add a fixed array to the entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="length"></param>
        /// <typeparam name="T"></typeparam>
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
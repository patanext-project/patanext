using System;
using System.Reflection;
using JetBrains.Annotations;
using Unity.Entities;
using UnityEngine;

namespace Packages.pack.guerro.shared.Scripts.Utilities
{
    public static class FastEntityGameObjectExtensions
    {
        [NotNull]
        private static MethodInfo s_WrapperDataGetComponentType;

        [NotNull]
        private static MethodInfo s_WrapperDataUpdateComponentData;

        static FastEntityGameObjectExtensions()
        {
            // ReSharper disable AssignNullToNotNullAttribute
            s_WrapperDataGetComponentType = typeof(ComponentDataWrapperBase)
                .GetMethod("GetComponentType", BindingFlags.NonPublic
                                               | BindingFlags.Instance);
            s_WrapperDataUpdateComponentData = typeof(ComponentDataWrapperBase)
                .GetMethod("UpdateComponentData", BindingFlags.NonPublic
                                                  | BindingFlags.Instance);

            foreach (var method in typeof(ComponentDataWrapperBase).GetMethods(BindingFlags.NonPublic
                                                                               | BindingFlags.Instance))
            {
                Debug.Log(method.Name);
            }

            Debug.Assert(s_WrapperDataGetComponentType != null, "s_WrapperDataGetComponentType == null");
            Debug.Assert(s_WrapperDataUpdateComponentData != null, "s_WrapperDataUpdateComponentData == null");
            // ReSharper restore AssignNullToNotNullAttribute
        }

        public static void AddComponentToEntity<TComponent>(this GameObject gameObject,
                                                            TComponent      value        = default(TComponent),
                                                            bool            updateEntity = true)
            where TComponent : Component
        {
            var entityGameObject = gameObject.GetComponent<GameObjectEntity>();
            if (entityGameObject != null)
                AddComponentToEntity(entityGameObject, value);
        }

        public static void AddComponentToEntity<TComponent>(this GameObjectEntity entityGameObject,
                                                            TComponent            value        = default(TComponent),
                                                            bool                  updateEntity = true)
            where TComponent : Component
        {
            AddComponentToEntity(entityGameObject, typeof(TComponent), value);
        }

        public static void AddComponentToEntity(this GameObjectEntity gameObjectEntity,
                                                Type                  componentType = null,
                                                Component             component     = default(Component),
                                                bool                  updateEntity  = true)
        {            
            if (componentType == null)
            {
                if (component == null)
                    throw new NullReferenceException(
                        $"'Parameters {nameof(component)} and '{nameof(componentType)}' are null.'");
                componentType = component.GetType();
            }


            if (!gameObjectEntity.GetComponent(componentType))
            {
                gameObjectEntity.gameObject.AddComponent(componentType);
            }

            var compWrapper = component as ComponentDataWrapperBase;

            var entity        = gameObjectEntity.Entity;
            var entityManager = gameObjectEntity.EntityManager;

            if (compWrapper != null)
            {
                var type = (ComponentType) s_WrapperDataGetComponentType.Invoke(compWrapper, null);

                if (!entityManager.HasComponent(entity, type))
                    entityManager.AddComponent(entity, type);

                var parameters = new object[] {entityManager, entity};
                s_WrapperDataUpdateComponentData.Invoke(compWrapper, parameters);
            }
            else
            {
                var type = new ComponentType(componentType);
                if (!entityManager.HasComponent(entity, type))
                    entityManager.AddComponent(entity, type);
            }

            if (updateEntity)
            {
                /*gameObjectEntity.OnDisable();
                gameObjectEntity.OnEnable();*/
                gameObjectEntity.Refresh();
            }
        }

        public static void CustomRefresh(this GameObject gameObject, GameObjectEntity gameObjectEntity = null)
        {
            gameObjectEntity = gameObjectEntity ?? gameObject.GetComponent<GameObjectEntity>();

            var components = gameObject.GetComponents<Component>();
            for (int i = 0; i != components.Length; ++i)
            {
                var comp = components[i];

                AddComponentToEntity(gameObjectEntity, comp, updateEntity: false);
            }
            
            gameObjectEntity.Refresh();
        }

        public static void CustomRefresh(this GameObjectEntity gameObjectEntity)
        {
            CustomRefresh(gameObjectEntity.gameObject, gameObjectEntity);
        }
    }
}
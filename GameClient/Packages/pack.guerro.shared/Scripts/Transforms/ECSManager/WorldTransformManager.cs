using System;
using Unity.Entities;
using UnityEngine;

namespace Packet.Guerro.Shared.Transforms
{
    /// <summary>
    /// A transform which combine 'Position' and 'Rotation' components. Don't use this struct on linear calculations.
    /// </summary>
    [Serializable]
    public struct DWorldTransformData
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public Vector3    Position;
        public Quaternion Rotation;

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Constructors
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public DWorldTransformData(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public DWorldTransformData(DWorldPositionData positionComponent, DWorldRotationData rotationComponent)
        {
            Position = positionComponent.Value;
            Rotation = rotationComponent.Value;
        }
    }

    public class STWorldTransformManager : ComponentSystem
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        protected override void OnUpdate()
        {
        }

        public bool UpdateTransform(Entity entity, DWorldTransformData information)
        {
            if (EntityManager.HasComponent<DWorldPositionData>(entity))
            {
                EntityManager.SetComponentData(entity, new DWorldPositionData(information.Position));
            }

            if (EntityManager.HasComponent<DWorldRotationData>(entity))
            {
                EntityManager.SetComponentData(entity, new DWorldRotationData(information.Rotation));
            }

            return true;
        }

        public bool UpdateTransform(Entity entity, DWorldPositionData informationPos, DWorldRotationData informationRot)
        {
            if (EntityManager.HasComponent<DWorldPositionData>(entity))
            {
                EntityManager.SetComponentData(entity, informationPos);
            }

            if (EntityManager.HasComponent<DWorldRotationData>(entity))
            {
                EntityManager.SetComponentData(entity, informationRot);
            }

            return true;
        }

        public bool UpdatePosition(Entity entity, DWorldPositionData information)
        {
            var hasEntity = EntityManager.HasComponent<DWorldPositionData>(entity);
            if (!hasEntity)
            {
                return false;
            }

            EntityManager.SetComponentData(entity, information);

            return true;
        }

        public bool UpdatePosition(Entity entity, Vector3 information)
        {
            var hasEntity = EntityManager.HasComponent<DWorldPositionData>(entity);
            if (!hasEntity)
            {
                return false;
            }

            EntityManager.SetComponentData(entity, new DWorldPositionData(information));

            return true;
        }

        public bool UpdateRotation(Entity entity, DWorldRotationData information)
        {
            var hasEntity = EntityManager.HasComponent<DWorldRotationData>(entity);
            if (!hasEntity)
            {
                return false;
            }

            EntityManager.SetComponentData(entity, information);

            return true;
        }

        public bool UpdateRotation(Entity entity, Quaternion information)
        {
            var hasEntity = EntityManager.HasComponent<DWorldRotationData>(entity);
            if (!hasEntity)
            {
                return false;
            }

            EntityManager.SetComponentData(entity, new DWorldRotationData(information));

            return true;
        }
    }
}
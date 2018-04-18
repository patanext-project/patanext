using System;
using Unity.Entities;
using UnityEngine;

namespace Packet.Guerro.Shared.Transforms
{
    /*
     * I didn't used the 'Position' and 'Rotation' components from Unity.Entities namespace
     * The main reason is that I wanted 100% full control with positions and rotations in RW
     * for the systems.
     */
    
    [Serializable]
    public struct DWorldRotationData : IComponentData
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public Quaternion Value;

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Constructors
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public DWorldRotationData(Quaternion value)
        {
            Value = value;
        }
    }

    [AddComponentMenu("Moddable/Transforms/World Rotation")]
    public class DWorldRotationWrapper : ComponentDataWrapper<DWorldRotationData>
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Unity Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        private void OnDrawGizmosSelected()
        {
            var goEntity = GetComponent<GameObjectEntity>();
            Value = goEntity.EntityManager.GetComponentData<DWorldRotationData>(goEntity.Entity);
        }
    }
}
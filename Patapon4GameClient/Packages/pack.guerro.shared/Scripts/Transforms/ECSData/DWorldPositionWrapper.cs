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
    public struct DWorldPositionData : IComponentData
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public Vector3 Value;

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Constructors
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public DWorldPositionData(Vector3 value)
        {
            Value = value;
        }
    }

    [AddComponentMenu("Moddable/Transforms/World Position")]
    public class DWorldPositionWrapper : ComponentDataWrapper<DWorldPositionData>
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Unity Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        private void OnDrawGizmosSelected()
        {
            var goEntity = GetComponent<GameObjectEntity>();

            Value = goEntity.EntityManager.GetComponentData<DWorldPositionData>(goEntity.Entity);

            Gizmos.color = Color.cyan;
            Gizmos.DrawMesh(CreateCube(), Value.Value - (new Vector3(0.5f, 0.5f, 0.5f) * 0.1f), Quaternion.identity,
                Vector3.one * 0.1f);
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        Mesh CreateCube()
        {
            Vector3[] vertices =
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(1, 1, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 1),
                new Vector3(1, 1, 1),
                new Vector3(1, 0, 1),
                new Vector3(0, 0, 1),
            };

            int[] triangles =
            {
                0, 2, 1, //face front
                0, 3, 2,
                2, 3, 4, //face top
                2, 4, 5,
                1, 2, 5, //face right
                1, 5, 6,
                0, 7, 4, //face left
                0, 4, 3,
                5, 4, 7, //face back
                5, 7, 6,
                0, 6, 7, //face bottom
                0, 1, 6
            };

            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.vertices  = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}
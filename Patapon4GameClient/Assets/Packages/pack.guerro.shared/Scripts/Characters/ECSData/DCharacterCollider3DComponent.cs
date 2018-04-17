using Unity.Entities;
using UnityEngine;

namespace Packet.Guerro.Shared.Characters
{
    /// <summary>
    ///     Base component for all 3D characters colliders
    /// </summary>
    public class DCharacterCollider3DComponent : DCharacterColliderComponentBase
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        /// <summary>
        ///     Reference to the hittable collider
        /// </summary>
        public Collider HitCollider;

        /// <summary>
        ///     Reference to the collider used for computing movement data
        /// </summary>
        public Collider MovementCollider;

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Base Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public override Vector3 FootPanelCenter()
        {
            if (MovementCollider is CapsuleCollider capsuleCollider)
                return new Vector3(capsuleCollider.center.x, FootPanel, capsuleCollider.center.z);

            // return bounds
            return new Vector3(MovementCollider.bounds.center.x, FootPanel, MovementCollider.bounds.center.z);
        }

        public override Vector3 FootPanelSize()
        {
            if (MovementCollider is CapsuleCollider capsuleCollider)
                return new Vector3(capsuleCollider.radius, FootPanel, capsuleCollider.radius);

            // return bounds
            return new Vector3(MovementCollider.bounds.extents.x, FootPanel, MovementCollider.bounds.extents.z);
        }
        
        public override Component Boxed_GetHitCollider()
        {
            return HitCollider;
        }

        public override Component Boxed_GetMovementCollider()
        {
            return MovementCollider;
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Unity Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        private void OnDrawGizmos()
        {
            if (MovementCollider == null)
                return;

            float    currentHeight = 0f, varPanel;
            Collider varCollider;

            var isGrounded = false;
            var goWrapper  = GetComponent<GameObjectEntity>();
            if (goWrapper.EntityManager.HasComponent<DCharacterData>(goWrapper.Entity))
            {
                var character = goWrapper.EntityManager.GetComponentData<DCharacterData>(goWrapper.Entity);
                isGrounded = character.IsGrounded;
            }

            // TODO
            /*varCollider  = MovementCollider;
            varPanel     = FootPanel;
            Gizmos.color = isGrounded ? new Color(0.05f, 0.8f, 0.04f, 0.75f) : new Color(0.05f, 0.09f, 0.8f, 0.75f);
            Gizmos.DrawCube(
                transform.position + new Vector3(varCollider.center.x, currentHeight + varPanel * 0.5f,
                    varCollider.center.z),
                new Vector3(varCollider.radius * 0.99f, varPanel - currentHeight, varCollider.radius * 0.99f));

            currentHeight += varPanel;

            varPanel     = BodyPanel;
            Gizmos.color = new Color(0.8f, 0.05f, 0.08f, 0.75f);
            Gizmos.DrawCube(
                transform.position + new Vector3(varCollider.center.x, (currentHeight + varPanel) * 0.5f,
                    varCollider.center.z),
                new Vector3(varCollider.radius * 0.99f, varPanel - currentHeight, varCollider.radius * 0.99f));*/
        }
    }
}
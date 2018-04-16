using UnityEngine;

namespace Packet.Guerro.Shared.Characters
{
    public class DCharacterCollider2DComponent : DCharacterColliderComponentBase
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public Collider2D HitCollider;
        public Collider2D MovementCollider;

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Base Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public override Vector3 FootPanelSize()
        {
            throw new System.NotImplementedException();
        }

        public override Vector3 FootPanelCenter()
        {
            throw new System.NotImplementedException();
        }

        public override Component Boxed_GetMovementCollider()
        {
            throw new System.NotImplementedException();
        }

        public override Component Boxed_GetHitCollider()
        {
            throw new System.NotImplementedException();
        }
    }
}
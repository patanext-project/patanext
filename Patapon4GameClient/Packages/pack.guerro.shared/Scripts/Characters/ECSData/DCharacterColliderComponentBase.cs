using UnityEngine;

namespace Packet.Guerro.Shared.Characters
{
    public abstract class DCharacterColliderComponentBase : MonoBehaviour
    {
        /// <summary>
        /// Reference to the position of the head (used for shooting, or for sending some rays, etc...)
        /// </summary>
        public float HeadPosition;
        /// <summary>
        /// Reference to the front panel of the foot (used for autojump and slide), height value, done from bottom of the collider
        /// </summary>
        public float FootPanel;
        /// <summary>
        /// Reference to the front panel of the body (used for walljump), height value, done from the bottom of the collider
        /// </summary>
        public float BodyPanel;
        /// <summary>
        /// Reference to the mesh that will be rotated by movements, etc...
        /// </summary>
        public Transform RotateGameObject;
        /// <summary>
        /// Is the character able to push other rigidbodies?
        /// </summary>
        public bool      CanPushRigidbodies;

        public abstract Vector3 FootPanelSize();
        public abstract Vector3 FootPanelCenter();
        public abstract Component Boxed_GetMovementCollider();
        public abstract Component Boxed_GetHitCollider();
    }
}
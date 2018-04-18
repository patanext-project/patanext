using System;
using Unity.Entities;
using UnityEngine;

namespace Packet.Guerro.Shared.Characters
{
    [Serializable]
    public struct DCharacterInformationData : IComponentData
    {
        /// <summary>
        /// Get the velocity of the character from the start of the frame
        /// <para>'<see cref="CurrentVelocity"/>' and this can be in the same value for Movements systems.</para>
        /// </summary>
        public Vector3 PreviousVelocity;

        /// <summary>
        /// Get the current velocity of the character.
        /// <para>'<see cref="PreviousVelocity"/>' and this can be in the same value for Movements systems.</para>
        /// </summary>
        public Vector3 CurrentVelocity;

        /// <summary>
        /// The previous position of the character (from frame beginning)
        /// </summary>
        public Vector3 PreviousPosition;
    }

    [AddComponentMenu("Moddable/Characters/Character Information")]
    public class DCharacterInformationWrapper : ComponentDataWrapper<DCharacterInformationData>
    {
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning($"> You don't need to add the component {nameof(DCharacterInformationWrapper)} in release.");
            }
        }
    }
}
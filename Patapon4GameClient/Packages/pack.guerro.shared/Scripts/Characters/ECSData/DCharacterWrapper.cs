using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Packet.Guerro.Shared.Characters
{
    [Serializable]
    public struct DCharacterData : IComponentData
    {
        /// <summary>
        /// Direction of the character.
        /// <para>Some warnings thoughts:</para>
        /// <para>In a 3D world, it will be a flat plane (XZ Y) related to the character</para>
        /// <para>In a 2D world, it will be a flat line (X Y) related to the character</para>
        /// </summary>
        [Header("Read-only")]
        public Vector3 Direction;
        /// <summary>
        /// The head rotation of the character.
        /// </summary>
        /// <para>In a 3D world, it will be related to the Y rotation of the character</para>
        /// <para>In a 2D world, it will be related to the Z rotation of the character</para>
        public float HeadRotation;
        /// <summary>
        /// Was the character grounded in the start of the frame?
        /// </summary>
        public bool1 WasGrounded;
        /// <summary>
        /// Is the character currently grounded?
        /// </summary>
        public bool1 IsGrounded;
        /// <summary>
        /// The fly time of the character.
        /// </summary>
        public float FlyTime;
        public Vector3 PreviousRunVelocity;
        public Vector3 RunVelocity;

        [Header("Properties")]
        public float MaximumStepAngle;
    }

    [AddComponentMenu("Moddable/Characters/Character")]
    public class DCharacterWrapper : ComponentDataWrapper<DCharacterData>
    {
        
    }
}
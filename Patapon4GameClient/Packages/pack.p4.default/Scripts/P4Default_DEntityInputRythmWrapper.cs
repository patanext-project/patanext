using System;
using Packet.Guerro.Shared.Inputs;
using Unity.Entities;

namespace P4.Default
{
    [Serializable]
    public struct P4Default_DEntityInputRythmData : IComponentData
    {
        /// <summary>
        /// The result input when the entity has pressed the 'Pata' key
        /// </summary>
        public P4Default_EEntityInputRythmActionState KeyAction1;

        /// <summary>
        /// The result input when the entity has pressed the 'Pon' key
        /// </summary>
        public P4Default_EEntityInputRythmActionState KeyAction2;

        /// <summary>
        /// The result input when the entity has pressed the 'Don' key
        /// </summary>
        public P4Default_EEntityInputRythmActionState KeyAction3;

        /// <summary>
        /// The result input when the entity has pressed the 'Chaka' key
        /// </summary>
        public P4Default_EEntityInputRythmActionState KeyAction4;
    }

    public class P4Default_DEntityInputRythmWrapper : ComponentDataWrapper<P4Default_DEntityInputRythmData>
    {
        
    }

    public enum P4Default_EEntityInputRythmActionState
    {
        None,
        Pressed,
    }
}
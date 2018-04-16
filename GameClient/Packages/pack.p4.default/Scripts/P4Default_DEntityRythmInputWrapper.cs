﻿using System;
using Packet.Guerro.Shared.Inputs;
using Unity.Entities;

namespace P4.Default
{
    [Serializable]
    public struct P4Default_DEntityRythmInputData : IComponentData
    {
        /// <summary>
        /// The result input when the entity has pressed the 'Pata' key
        /// </summary>
        public SInputResult LeftRythmKey;

        /// <summary>
        /// The result input when the entity has pressed the 'Pon' key
        /// </summary>
        public SInputResult RightRythmKey;

        /// <summary>
        /// The result input when the entity has pressed the 'Don' key
        /// </summary>
        public SInputResult DownRythmKey;

        /// <summary>
        /// The result input when the entity has pressed the 'Chaka' key
        /// </summary>
        public SInputResult UpRythmKey;
    }

    public class P4Default_DEntityRythmInputWrapper : ComponentDataWrapper<P4Default_DEntityRythmInputData>
    {

    }
}
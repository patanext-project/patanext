using System;
using Unity.Entities;

namespace P4.Default
{
    /// <summary>
    /// This struct combine the inputs from an entity into one input struct.
    /// </summary>
    [Serializable]
    public struct P4Default_DEntityInputUnifiedData : IComponentData
    {
        public P4Default_DEntityInputFreeData  FreeInput;
        public P4Default_DEntityInputRythmData RythmInput;
    }
    
    public class P4Default_DEntityInputUnifiedWrapper : ComponentDataWrapper<P4Default_DEntityInputUnifiedData>
    {
    }
}
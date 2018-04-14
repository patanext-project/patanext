using System;
using Unity.Entities;

namespace P4.Default
{
    [Serializable]
    public struct P4Default_DEntityInputData : IComponentData
    {
        
    }
    
    public class P4Default_DEntityInputWrapper : ComponentDataWrapper<P4Default_DEntityInputData>
    {
        
    }
}
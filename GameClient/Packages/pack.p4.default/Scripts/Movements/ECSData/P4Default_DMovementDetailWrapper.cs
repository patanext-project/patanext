using System;
using Unity.Entities;
using UnityEngine;

namespace P4.Default.Movements
{
    [Serializable]
    public struct P4Default_DMovementDetailData : IComponentData
    {
    }

    [AddComponentMenu("Moddable/P4Default/Movement Detail")]
    public class P4Default_DMovementDetailWrapper : ComponentDataWrapper<P4Default_DMovementDetailData>
    {
    }
}
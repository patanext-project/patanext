using System;
using Unity.Entities;

namespace P4Main.Components
{
    [Serializable]
    public struct InputReceiver : IComponentData
    {

    }

    public class InputReceiverComponent : ComponentDataWrapper<InputReceiver>
    {

    }
}
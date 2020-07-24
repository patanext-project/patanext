using System;
using System.Collections.ObjectModel;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Inputs.DefaultActions;
using GameHost.Inputs.Interfaces;
using GameHost.Inputs.Layouts;
using GameHost.Inputs.Systems;
using GameHost.Worlds.Components;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Game.Inputs.Actions
{
    public struct RhythmInputAction : IInputAction
    {
        // use the same layout as the PressAction one
        public class Layout : PressAction.Layout
        {
            public Layout(string id, params CInput[] inputs) : base(id, inputs)
            {
            }
        }

        public uint DownCount, UpCount;
        public bool Active;

        public TimeSpan ActiveTime;

        public bool HasBeenPressed => DownCount > 0;
        public bool IsSliding      => ActiveTime.TotalSeconds > 0.4;

        public class System : InputActionSystemBase<RhythmInputAction, Layout>
        {
            public System(WorldCollection collection) : base(collection)
            {
            }

            // Only reset Down and Up count.
            public override void OnBeginFrame()
            {
                foreach (var entity in InputQuery.GetEntities())
                {
                    ref var current = ref entity.Get<RhythmInputAction>();
                    current.DownCount = 0;
                    current.UpCount   = 0;
                }
            }
        }

        public void Serialize(ref DataBufferWriter buffer)
        {
            buffer.WriteValue(DownCount);
            buffer.WriteValue(UpCount);
            buffer.WriteValue(Active);
            buffer.WriteValue(ActiveTime);
        }

        public void Deserialize(ref DataBufferReader buffer)
        {
            DownCount  += buffer.ReadValue<uint>();
            UpCount    += buffer.ReadValue<uint>();
            Active     = buffer.ReadValue<bool>();
            ActiveTime = buffer.ReadValue<TimeSpan>();
        }
    }
}
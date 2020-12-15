using System;
using GameHost.Core.Ecs;
using GameHost.Inputs.DefaultActions;
using GameHost.Inputs.Interfaces;
using GameHost.Inputs.Layouts;
using GameHost.Inputs.Systems;
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
                    if (current.DownCount > 0)
                        Console.WriteLine("RESET");
                    
                    current.DownCount = 0;
                    current.UpCount   = 0;
                }
            }

            public override void OnInputUpdate()
            {
                var currentLayout = World.Mgr.Get<InputCurrentLayout>()[0];
                foreach (var entity in InputQuery.GetEntities())
                {
                    var layouts = GetLayouts(entity);
                    
                    if (!layouts.TryGetOrDefault(currentLayout.Id, out var layout) || !(layout is Layout axisLayout))
                        return;
                    
                    ref var action = ref entity.Get<RhythmInputAction>();
                    action.DownCount = 0;
                    action.UpCount   = 0;
                    action.Active    = false;

                    for (var i = 0; i < layout.Inputs.Count; i++)
                    {
                        var input = layout.Inputs[i];
                        if (Backend.GetInputControl(input.Target) is {} buttonControl)
                        {
                            action.DownCount += buttonControl.wasPressedThisFrame ? 1u : 0;
                            action.UpCount   += buttonControl.wasReleasedThisFrame ? 1u : 0;
                            action.Active    |= buttonControl.isPressed;
                        }
                    }
                    
                    /*if (action.Active)
                        action.ActiveTime += World.Mgr.Get<WorldTime>()[0].Delta;
                    else
                        action.ActiveTime = TimeSpan.Zero;*/
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
            Active     =  buffer.ReadValue<bool>();
            ActiveTime =  buffer.ReadValue<TimeSpan>();

            if (DownCount > 0)
                Console.WriteLine(DownCount);
        }
    }
}
using System;
using System.Collections.ObjectModel;
using GameHost.Core.Ecs;
using GameHost.Entities;
using GameHost.Input;
using GameHost.Input.Default;

namespace PataponGameHost.Inputs
{
    public struct RhythmInputAction : IInputAction
    {
        public class Layout : InputLayoutBase
        {
            public Layout(string id, params CInput[] inputs) : base(id)
            {
                Inputs = new ReadOnlyCollection<CInput>(inputs);
            }
        }

        public uint DownCount, UpCount;
        public bool Active;
        
        public TimeSpan ActiveTime;

        public bool HasBeenPressed => DownCount > 0;
        public bool IsSliding => ActiveTime.TotalSeconds > 0.3;

        public class Provider : InputProviderSystemBase<Provider, RhythmInputAction>
        {
            private IManagedWorldTime wt;
            
            protected override void OnInputThreadUpdate()
            {
                var currentLayout = World.Mgr.Get<InputCurrentLayout>()[0];
                lock (InputSynchronizationBarrier)
                {
                    foreach (ref readonly var entity in InputSet.GetEntities())
                    {
                        var layouts = entity.Get<InputActionLayouts>();
                        if (!layouts.TryGetOrDefault(currentLayout.Id, out var layout))
                            continue;

                        ref var action = ref entity.Get<RhythmInputAction>();

                        var wasActive = action.Active;
                        action.Active = false;
                        foreach (var input in layout.Inputs)
                        {
                            action.DownCount += Backend.GetInputState(input.Target).Down;
                            action.UpCount   += Backend.GetInputState(input.Target).Up;
                            action.Active    |= Backend.GetInputState(input.Target).Active;
                        }

                        if (action.Active && wasActive)
                        {
                            action.ActiveTime += wt.Delta;
                        }
                        else
                        {
                            action.ActiveTime = TimeSpan.Zero;
                        }
                    }
                }
            }

            protected override void OnReceiverUpdate()
            {
                lock (InputSynchronizationBarrier)
                {
                    foreach (ref readonly var entity in InputSet.GetEntities())
                    {
                        ref var inputFromThread = ref entity.Get<InputThreadTarget>().Target.Get<RhythmInputAction>();
                        ref var selfInput       = ref entity.Get<RhythmInputAction>();

                        selfInput.DownCount  = inputFromThread.DownCount;
                        selfInput.UpCount    = inputFromThread.UpCount;
                        selfInput.Active     = inputFromThread.Active;
                        selfInput.ActiveTime = inputFromThread.ActiveTime;

                        inputFromThread.DownCount = 0;
                        inputFromThread.UpCount   = 0;
                    }
                }
            }

            public Provider(WorldCollection collection) : base(collection)
            {
                DependencyResolver.Add(() => ref wt);
            }
        }
    }
}
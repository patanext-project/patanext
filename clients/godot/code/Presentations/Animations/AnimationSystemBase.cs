using Godot;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using revecs.Core;
using revecs.Core.Components.Boards;
using revecs.Extensions.Generator.Components;
using revghost;

namespace PataNext.Presentations.Animations;

public abstract class AnimationSystemBase<TData> : SimulationSystem
    where TData : IRevolutionComponent
{
    public ComponentType<TData> SystemType { get; private set; }

    protected AnimationSystemBase(Scope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        SystemType = Simulation.AsGenericComponentType<TData>(TData.ToComponentType(Simulation));
    }

    protected abstract void OnAnimationUpdate(UEntityHandle entity, AnimationComponent component);
}
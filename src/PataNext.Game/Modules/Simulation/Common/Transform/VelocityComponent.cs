using System.Numerics;
using System.Runtime.CompilerServices;
using revecs.Extensions.Generator.Components;

namespace PataNext.Game.Modules.Simulation.Common.Transform;

public partial struct VelocityComponent : ISparseComponent
{
    public Vector2 Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VelocityComponent(Vector2 value)
    {
        Value = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VelocityComponent(float x, float y)
    {
        Value.X = x;
        Value.Y = x;
    }
}
using System;
using System.Numerics;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.CoreAbilities.Mixed.CYari
{
    public interface IThrowSpearAbility : ISimpleAttackAbility
    {
        public Vector2 ThrowVelocity { get; }
        public Vector2 Gravity { get; }
    }
}
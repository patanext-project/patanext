using System;
using System.Collections.Generic;
using System.Numerics;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Worlds.Components;
using PataNext.CoreAbilities.Mixed;
using PataNext.CoreAbilities.Mixed.CYumi;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.GamePlay;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Server.Yumi.AttackCommand
{
    public class YumiyachaSnipeAttack : ScriptBase<YumiyachaSnipeAttackAbilityProvider>
    {
        private IManagedWorldTime          worldTime;
        private SpearProjectileProvider    projectileProvider;
        private ExecuteActiveAbilitySystem execute;

        public YumiyachaSnipeAttack(WorldCollection collection) : base(collection)
        {
            DependencyResolver.Add(() => ref worldTime);
            DependencyResolver.Add(() => ref projectileProvider);
            DependencyResolver.Add(() => ref execute);
        }

        protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
        {
            ref var ability = ref GetComponentData<YumiyachaSnipeAttackAbility>(self);
            ability.Cooldown -= worldTime.Delta;

            ref var controlVelocity = ref GetComponentData<AbilityControlVelocity>(self);

            ref readonly var position  = ref GetComponentData<Position>(owner).Value;
            ref readonly var playState = ref GetComponentData<UnitPlayState>(owner);
            ref readonly var direction = ref GetComponentData<UnitDirection>(owner);
            ref readonly var offset    = ref GetComponentData<UnitTargetOffset>(owner);

            var throwOffset = new Vector2(direction.Value, 1f);

            if (!state.IsActiveOrChaining)
            {
                ability.StopAttack();
                return;
            }

            // If true, we're currently in the attack phase
            if (ability.IsAttackingAndUpdate(worldTime.Total))
            {
                // When we attack, we should stay at our current position, and add a very small deceleration 
                controlVelocity.StayAtCurrentPositionX(1);

                if (ability.CanAttackThisFrame(worldTime.Total, TimeSpan.FromSeconds(playState.AttackSpeed)))
                {
                    // Todo: accuracy should be stored in UnitPlayState (so we could have spears/skills that would be more precise)
                    var accuracy       = AbilityUtility.CompileStat(GetComponentData<AbilityEngineSet>(self), 0.2f, 1, 2.5, 1.5);
                    var throwOffsetXYZ = new Vector3(throwOffset, 0);
                    execute.Post.Schedule(projectileProvider.SpawnAndForget, (owner,
                        position + throwOffsetXYZ,
                        new Vector3 {X = ability.ThrowVelocity.X, Y = ability.ThrowVelocity.Y + accuracy * (float) random.NextDouble()},
                        new Vector3(ability.Gravity, 0)), default);
                }
            }
            else if (state.IsChaining)
                controlVelocity.StayAtCurrentPositionX(50);

            var (enemyPrioritySelf, _) = GetNearestEnemy(owner.Handle, 4, null);
            if (state.IsActive && enemyPrioritySelf != default)
            {
                var targetPosition = GetComponentData<Position>(enemyPrioritySelf).Value;
                var deltaPosition  = PredictTrajectory.Simple(throwOffset - Vector2.UnitX * offset.Attack, ability.ThrowVelocity, ability.Gravity);
                // Search for any weakpoint the enemy has, and if it does, add it to the deltaPosition var
                if (TryGetComponentBuffer<UnitWeakPoint>(enemyPrioritySelf, out var weakPoints)
                    && weakPoints.GetNearest(targetPosition - position) is var (weakPoint, dist) && dist >= 0)
                    deltaPosition += weakPoint.XY();

                targetPosition.X -= deltaPosition.X;

                if (ability.AttackStart == default)
                    controlVelocity.SetAbsolutePositionX(targetPosition.X, 50);

                // We should have a mercy in the distance of where the unit is and where it should throw. (it shouldn't be able to only throw at a perfect position)
                const float distanceMercy = 4f;
                // If we're near enough of where we should throw the spear, throw it.
                if (MathF.Abs(targetPosition.X - position.X) < distanceMercy && ability.TriggerAttack(worldTime.ToStruct()))
                {
                }

                controlVelocity.OffsetFactor = 0;
            }
        }


        private Random random = new Random(Environment.TickCount);

        protected override void OnSetup(Span<GameEntityHandle> abilities)
        {
            random.Next();
        }

        public override void Dispose()
        {
            projectileProvider = null;
        }
    }
}
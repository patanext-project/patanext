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
    public class YumiyachaBasicAttack : ScriptBase<YumiyachaBasicAttackAbilityProvider>
    {
        private IManagedWorldTime          worldTime;
        private SpearProjectileProvider    projectileProvider;
        private ExecuteActiveAbilitySystem execute;

        public YumiyachaBasicAttack(WorldCollection collection) : base(collection)
        {
            DependencyResolver.Add(() => ref worldTime);
            DependencyResolver.Add(() => ref projectileProvider);
            DependencyResolver.Add(() => ref execute);
        }

        protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
        {
            ref var ability = ref GetComponentData<YumiyachaBasicAttackAbility>(self);
            ability.Cooldown -= worldTime.Delta;

            if (!state.IsActiveOrChaining)
            {
                ability.AccumulatedAccuracy -= (float) worldTime.Delta.TotalSeconds;
                ability.StopAttack();
                return;
            }
            
            ref var controlVelocity = ref GetComponentData<AbilityControlVelocity>(self);
            
            ref readonly var position  = ref GetComponentData<Position>(owner).Value;
            ref readonly var playState = ref GetComponentData<UnitPlayState>(owner);
            ref readonly var direction = ref GetComponentData<UnitDirection>(owner);
            ref readonly var offset    = ref GetComponentData<UnitTargetOffset>(owner);

            var throwOffset = new Vector2(direction.Value, 1.325f);
            if (state.IsActive)
            {
                var deltaPosition = PredictTrajectory.Simple(throwOffset - Vector2.UnitX * offset.Attack, ability.ThrowVelocity * direction.FactorX, ability.Gravity);
                var result        = RoutineGetNearOfEnemy(owner, deltaPosition, attackNearDistance: float.MaxValue, selfDistance: 4);
                if (result.Enemy != default)
                {
                    if (result.CanTriggerAttack)
                        ability.TriggerAttack(worldTime.ToStruct());

                    if (ability.AttackStart == default)
                        controlVelocity.SetAbsolutePositionX(result.Target.X, 20);

                    controlVelocity.OffsetFactor = 0;
                }
            }
            
            // If true, we're currently in the attack phase
            if (ability.IsAttackingAndUpdate(worldTime.Total))
            {
                // When we attack, we should stay at our current position, and add a very small deceleration 
                controlVelocity.StayAtCurrentPositionX(1);

                if (ability.CanAttackThisFrame(worldTime.Total, TimeSpan.FromSeconds(playState.AttackSpeed)))
                {
                    if (ability.AccumulatedAccuracy < 0)
                        ability.AccumulatedAccuracy = 0;
                    
                    // Todo: accuracy should be stored in UnitPlayState (so we could have spears/skills that would be more precise)
                    var accuracy       = AbilityUtility.CompileStat(GetComponentData<AbilityEngineSet>(self), 0.2f + ability.AccumulatedAccuracy, 1, 2.5, 1.5);
                    var throwOffsetXYZ = new Vector3(throwOffset, 0);
                    execute.Post.Schedule(projectileProvider.SpawnAndForget, (owner,
                        position + throwOffsetXYZ,
                        new Vector3 {X = ability.ThrowVelocity.X, Y = ability.ThrowVelocity.Y + accuracy * (float) random.NextDouble()},
                        new Vector3(ability.Gravity, 0)), default);

                    ability.AccumulatedAccuracy += 0.1f;
                }
            }
            else if (state.IsChaining)
            {
                controlVelocity.StayAtCurrentPositionX(10);
                ability.AccumulatedAccuracy -= (float) worldTime.Delta.TotalSeconds * 0.1f;
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
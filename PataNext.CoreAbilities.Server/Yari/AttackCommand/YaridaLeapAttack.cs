using System;
using System.Collections.Generic;
using System.Numerics;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Worlds.Components;
using PataNext.CoreAbilities.Mixed;
using PataNext.CoreAbilities.Mixed.CYari;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.GamePlay;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Server.Yari.AttackCommand
{
    public class YaridaLeapAttack : ScriptBase<YaridaLeapAttackAbilityProvider>
    {
        private IManagedWorldTime          worldTime;
        private SpearProjectileProvider    projectileProvider;
        private ExecuteActiveAbilitySystem execute;

        public YaridaLeapAttack(WorldCollection collection) : base(collection)
        {
            DependencyResolver.Add(() => ref worldTime);
            DependencyResolver.Add(() => ref projectileProvider);
            DependencyResolver.Add(() => ref execute);
        }

        protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
        {
            ref var abilitySettings = ref GetComponentData<YaridaLeapAttackAbility>(self);
            ref var abilityState    = ref GetComponentData<YaridaLeapAttackAbility.State>(self);
            abilityState.Cooldown -= worldTime.Delta;

            if (!state.IsActiveOrChaining)
            {
                abilityState.StopAttack();
                return;
            }

            ref var controlVelocity = ref GetComponentData<AbilityControlVelocity>(self);
            ref var velocity        = ref GetComponentData<Velocity>(owner).Value;

            ref readonly var position  = ref GetComponentData<Position>(owner).Value;
            ref readonly var playState = ref GetComponentData<UnitPlayState>(owner);
            ref readonly var direction = ref GetComponentData<UnitDirection>(owner);
            ref readonly var offset    = ref GetComponentData<UnitTargetOffset>(owner);

            var throwOffset = new Vector2(direction.Value, 1.2f);
            if (state.IsActive)
            {
                var displacement = PredictTrajectory.GetDisplacement(new Vector2(0, 16), new Vector2(0, -26), (float) abilitySettings.DelayBeforeAttack.TotalSeconds);
                
                var deltaPosition = PredictTrajectory.Simple(throwOffset - Vector2.UnitX * offset.Attack + displacement,
                    abilitySettings.ThrowVelocity * direction.FactorX, 
                    abilitySettings.Gravity,
                    yLimit: 0.25f);
                var result        = RoutineGetNearOfEnemy(owner, deltaPosition, attackNearDistance: float.MaxValue, selfDistance: 4);
                if (result.Enemy != default)
                {
                    if (result.CanTriggerAttack && abilityState.TriggerAttack(worldTime.ToStruct()))
                    {
                        velocity.Y = MathF.Max(velocity.Y + 18, 18);
                    }

                    if (abilityState.AttackStart == default)
                        controlVelocity.SetAbsolutePositionX(result.Target.X, 50);

                    controlVelocity.OffsetFactor = 0;
                }
            }

            // If true, we're currently in the attack phase
            if (abilityState.IsAttackingAndUpdate(abilitySettings, worldTime.Total))
            {
                // When we attack, we should stay at our current position, and add a very small deceleration 
                controlVelocity.StayAtCurrentPositionX(1);

                if (abilityState.CanAttackThisFrame(abilitySettings, worldTime.Total, TimeSpan.FromSeconds(playState.AttackSpeed)))
                {
                    // Todo: accuracy should be stored in UnitPlayState (so we could have spears/skills that would be more precise)
                    var accuracy       = AbilityUtility.CompileStat(GetComponentData<AbilityEngineSet>(self), 0.01f, 1, 2.5, 1.75);
                    var throwOffsetXYZ = new Vector3(throwOffset, 0);
                    execute.Post.Schedule(projectileProvider.SpawnAndForget,
                        (owner,
                            position + throwOffsetXYZ,
                            new Vector3 {X = abilitySettings.ThrowVelocity.X * direction.Value, Y = abilitySettings.ThrowVelocity.Y + accuracy * (float) random.NextDouble()},
                            new Vector3(abilitySettings.Gravity, 0) * 0.9f
                        ), default);
                    
                    velocity.Y = MathUtils.LerpNormalized(velocity.Y, 0, 0.5f);
                }

                if (abilityState.DidAttack)
                {
                    controlVelocity.ActiveInAir = true;
                    controlVelocity.StayAtCurrentPositionX(10);
                }
            }
            else if (state.IsChaining)
                controlVelocity.StayAtCurrentPositionX(10);
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
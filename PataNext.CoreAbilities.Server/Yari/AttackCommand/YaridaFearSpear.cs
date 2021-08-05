using System;
using System.Collections.Generic;
using System.Numerics;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Worlds.Components;
using PataNext.CoreAbilities.Mixed;
using PataNext.CoreAbilities.Mixed.CYari;
using PataNext.CoreAbilities.Mixed.Subset;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.GamePlay;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Server.Yari.AttackCommand
{
    public class YaridaFearSpear : ScriptBase<YaridaFearSpearAbilityProvider>
    {
        private IManagedWorldTime           worldTime;
        private FearSpearProjectileProvider projectileProvider;
        private ExecuteActiveAbilitySystem  execute;

        public YaridaFearSpear(WorldCollection collection) : base(collection)
        {
            DependencyResolver.Add(() => ref worldTime);
            DependencyResolver.Add(() => ref projectileProvider);
            DependencyResolver.Add(() => ref execute);
        }

        protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
        {
            ref var abilitySettings = ref GetComponentData<YaridaFearSpearAbility>(self);
            ref var abilityState    = ref GetComponentData<YaridaFearSpearAbility.State>(self);
            abilityState.Cooldown -= worldTime.Delta;

            if (!state.IsActiveOrChaining)
            {
                abilityState.StopAttack();
                return;
            }

            ref var controlVelocity = ref GetComponentData<AbilityControlVelocity>(self);
            ref var velocity        = ref GetComponentData<Velocity>(owner).Value;

            ref var subset = ref GetComponentData<DefaultSubsetMarch>(self);
            subset.IsActive = (state.Phase & EAbilityPhase.Active) != 0 && HasComponent<MarchCommand>(GetComponentData<AbilityEngineSet>(self).Command.Entity);

            ref readonly var position  = ref GetComponentData<Position>(owner).Value;
            ref readonly var playState = ref GetComponentData<UnitPlayState>(owner);
            ref readonly var direction = ref GetComponentData<UnitDirection>(owner);
            ref readonly var offset    = ref GetComponentData<UnitTargetOffset>(owner);

            var throwOffset   = new Vector2(direction.Value * 2.5f, 1.2f);
            var deltaPosition = PredictTrajectory.Simple(throwOffset - Vector2.UnitX * offset.Attack + new Vector2(0, 2f),
                abilitySettings.ThrowVelocity * direction.FactorX,
                abilitySettings.Gravity);
            var result        = RoutineGetNearOfEnemy(owner, deltaPosition, attackNearDistance: float.MaxValue, selfDistance: 4);
            if (result.Enemy != default)
            {
                if (result.CanTriggerAttack && abilityState.TriggerAttack(worldTime.ToStruct()))
                {
                    velocity.Y = Math.Max(velocity.Y + 24, 24);
                }

                if (abilityState.AttackStart == default)
                    controlVelocity.SetAbsolutePositionX(result.Target.X, 15);

                controlVelocity.OffsetFactor = 0;
            }

            // If true, we're currently in the attack phase
            if (!abilityState.IsAttackingAndUpdate(abilitySettings, worldTime.Total)) 
                return;
            
            velocity.Y -= (float) worldTime.Delta.TotalSeconds * 14f;

            if (abilityState.CanAttackThisFrame(abilitySettings, worldTime.Total, TimeSpan.FromSeconds(playState.AttackSpeed)))
            {
                // Todo: accuracy should be stored in UnitPlayState (so we could have spears/skills that would be more precise)
                var accuracy       = 2.5f;
                var throwOffsetXYZ = new Vector3(throwOffset, 0);
                execute.Post.Schedule(projectileProvider.SpawnAndForget,
                    (owner,
                        position + throwOffsetXYZ - new Vector3(0, 0.25f, 0),
                        new Vector3 {X = abilitySettings.ThrowVelocity.X * direction.Value, Y = abilitySettings.ThrowVelocity.Y + accuracy * (float) random.NextDouble()},
                        new Vector3(abilitySettings.Gravity * 1.3f, 0)
                    ), default);
            }
            else if (!abilityState.DidAttack)
            {
                velocity.Y -= (float) worldTime.Delta.TotalSeconds * 27.5f;

                // the bottom lines would only work if we're on ground (in case we got pushed down, and we wouldn't want to sliiiiiide)
                controlVelocity.ActiveInAir = false;
                controlVelocity.StayAtCurrentPositionX(1);
            }

            if (abilityState.DidAttack)
                controlVelocity.StayAtCurrentPositionX(25f);
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
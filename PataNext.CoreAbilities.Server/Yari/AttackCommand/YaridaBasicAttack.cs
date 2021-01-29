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
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Server.Yari.AttackCommand
{
    public class YaridaBasicAttack : ScriptBase<YaridaBasicAttackAbilityProvider>
    {
        private IManagedWorldTime          worldTime;
        private SpearProjectileProvider    projectileProvider;
        private ExecuteActiveAbilitySystem execute;

        public YaridaBasicAttack(WorldCollection collection) : base(collection)
        {
            DependencyResolver.Add(() => ref worldTime);
            DependencyResolver.Add(() => ref projectileProvider);
            DependencyResolver.Add(() => ref execute);
        }

        protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
        {
            ref var abilitySettings = ref GetComponentData<YaridaBasicAttackAbility>(self);
            ref var abilityState    = ref GetComponentData<YaridaBasicAttackAbility.State>(self);
            abilityState.Cooldown -= worldTime.Delta;

            if (!state.IsActiveOrChaining)
            {
                abilityState.StopAttack();
                return;
            }

            ref var controlVelocity = ref GetComponentData<AbilityControlVelocity>(self);

            ref readonly var position  = ref GetComponentData<Position>(owner).Value;
            ref readonly var playState = ref GetComponentData<UnitPlayState>(owner);
            ref readonly var direction = ref GetComponentData<UnitDirection>(owner);
            ref readonly var offset    = ref GetComponentData<UnitTargetOffset>(owner);

            var throwOffset = new Vector2(direction.Value, 1.25f);
            if (state.IsActive)
            {
                var deltaPosition = PredictTrajectory.Simple(throwOffset - Vector2.UnitX * offset.Attack, abilitySettings.ThrowVelocity * direction.FactorX, abilitySettings.Gravity);
                var result        = RoutineGetNearOfEnemy(owner, deltaPosition, attackNearDistance: float.MaxValue, selfDistance: 4);
                if (result.Enemy != default)
                {
                    if (result.CanTriggerAttack)
                        abilityState.TriggerAttack(worldTime.ToStruct());

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
                    var accuracy       = AbilityUtility.CompileStat(GetComponentData<AbilityEngineSet>(self), 0.5f, 1, 2.5, 1.75);
                    var throwOffsetXYZ = new Vector3(throwOffset, 0);
                    execute.Post.Schedule(projectileProvider.SpawnAndForget,
                        (owner,
                            position + throwOffsetXYZ,
                            new Vector3 {X = abilitySettings.ThrowVelocity.X, Y = abilitySettings.ThrowVelocity.Y + accuracy * (float) random.NextDouble()},
                            new Vector3(abilitySettings.Gravity, 0)
                        ), default);
                }
            }
            else if (state.IsChaining)
                controlVelocity.StayAtCurrentPositionX(10);
        }


        private Random random = new Random(Environment.TickCount);

        protected override void OnSetup(GameEntity self)
        {
            random.Next();
        }

        public override void Dispose()
        {
            projectileProvider = null;
        }
    }
}
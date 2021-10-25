using System;
using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Worlds.Components;
using PataNext.CoreAbilities.Mixed;
using PataNext.CoreAbilities.Mixed.CTate;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using PataNext.Module.Simulation.Game.GamePlay.Damage;
using PataNext.Simulation.mixed.Components.GamePlay.Abilities;
using StormiumTeam.GameBase.GamePlay.HitBoxes;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Server.Tate.AttackCommand
{
    public class TaterazayBasicAttack : ScriptBase<TaterazayBasicAttackAbilityProvider>
    {
        private IManagedWorldTime           worldTime;
        private IPhysicsSystem              physicsSystem;
        private ExecuteActiveAbilitySystem  execute;
        private SetStatusEffectBufferHelper setStatusHelper;

        public TaterazayBasicAttack(WorldCollection collection) : base(collection)
        {
            DependencyResolver.Add(() => ref worldTime);
            DependencyResolver.Add(() => ref physicsSystem);
            DependencyResolver.Add(() => ref execute);
            DependencyResolver.Add(() => ref setStatusHelper);
        }

        protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
        {
            ref var abilityState    = ref GetComponentData<TaterazayBasicAttackAbility.State>(self);
            ref var abilitySettings = ref GetComponentData<TaterazayBasicAttackAbility>(self);
            abilityState.Cooldown -= worldTime.Delta;

            ref var controlVelocity = ref GetComponentData<AbilityControlVelocity>(self);

            ref readonly var position   = ref GetComponentData<Position>(owner).Value;
            ref readonly var statistics = ref GetComponentData<UnitStatistics>(owner);
            ref readonly var playState  = ref GetComponentData<UnitPlayState>(owner);
            
            GetBuffer<HitBoxHistory>(self).Clear();
            
            if (!state.IsActiveOrChaining)
            {
                abilityState.StopAttack();
                return;
            }

            var meleeRange = Math.Max(statistics.AttackMeleeRange, 2);

            // If true, we're currently in the attack phase
            if (abilityState.IsAttackingAndUpdate(abilitySettings, worldTime.Total))
            {
                // When we attack, we should stay at our current position, and add a very small deceleration 
                controlVelocity.StayAtCurrentPositionX(1);

                if (abilityState.CanAttackThisFrame(abilitySettings, worldTime.Total, TimeSpan.FromSeconds(playState.AttackSpeed)))
                {
                    execute.Post.Schedule(() =>
                    {
                        using var entitySettings = World.Mgr.CreateEntity();
                        entitySettings.Set<Shape>(new PolygonShape(meleeRange + 0.2f, meleeRange * 0.5f));

                        physicsSystem.AssignCollider(self.Handle, entitySettings);

                        // attack code
                        AddComponent(self, new HitBox(owner, default));

                        setStatusHelper.Set(owner.Handle, self.Handle);

                        Console.WriteLine("Slash!!!");
                    }, default);
                    
                    GetComponentData<Position>(self).Value       = position + new Vector3(0, meleeRange * 0.5f, 0);
                    GetComponentData<DamageFrameData>(self)      = new DamageFrameData(playState);
                    GetComponentData<HitBoxAgainstEnemies>(self) = new HitBoxAgainstEnemies(GetComponentData<Relative<TeamDescription>>(owner).Target);
                }
            }
            else if (state.IsChaining)
                controlVelocity.StayAtCurrentPositionX(50);

            var (enemyPrioritySelf, dist) = GetNearestEnemy(owner.Handle, 2, null);
            if (state.IsActive && enemyPrioritySelf != default)
            {
                var targetPosition = GetComponentData<Position>(enemyPrioritySelf).Value;
                if (abilityState.AttackStart == default)
                    controlVelocity.SetAbsolutePositionX(targetPosition.X, 50);

                // We should have a mercy in the distance of where the unit is and where it should throw. (it shouldn't be able to only throw at a perfect position)
                float distanceMercy = meleeRange + 1f;
                // If we're near enough of where we should throw the spear, throw it.
                if (MathF.Abs(targetPosition.X - position.X) < distanceMercy && abilityState.TriggerAttack(worldTime.ToStruct()))
                {
                }

                controlVelocity.OffsetFactor = 0;
            }
        }

        protected override void OnSetup(Span<GameEntityHandle> abilities)
        {
            foreach (var handle in abilities)
            {
                if (GameWorld.RemoveComponent(handle, AsComponentType<HitBox>()))
                {
                    if (GameWorld.GetBuffer<HitBoxHistory>(handle).Count > 0)
                        Console.WriteLine("Has slashed something!!!");
                    else
                        Console.WriteLine("Didn't slashed anything :(");
                }
            }
        }
    }
}
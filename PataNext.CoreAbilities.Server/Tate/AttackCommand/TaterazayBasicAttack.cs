using System;
using System.Numerics;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Worlds.Components;
using PataNext.CoreAbilities.Mixed;
using PataNext.CoreAbilities.Mixed.CTate;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using StormiumTeam.GameBase.GamePlay.HitBoxes;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Server.Tate.AttackCommand
{
    public class TaterazayBasicAttack : ScriptBase<TaterazayBasicAttackAbilityProvider>
    {
        private IManagedWorldTime          worldTime;
        private ExecuteActiveAbilitySystem execute;

        public TaterazayBasicAttack(WorldCollection collection) : base(collection)
        {
            DependencyResolver.Add(() => ref worldTime);
            DependencyResolver.Add(() => ref execute);
        }

        protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
        {
            ref var ability = ref GetComponentData<TaterazayBasicAttackAbility>(self);
            ability.Cooldown -= worldTime.Delta;

            ref var controlVelocity = ref GetComponentData<AbilityControlVelocity>(self);

            ref readonly var position     = ref GetComponentData<Position>(owner).Value;
            ref readonly var playState    = ref GetComponentData<UnitPlayState>(owner);

            GameWorld.RemoveComponent(self.Handle, AsComponentType<HitBox>());
            GetBuffer<HitBoxHistory>(self).Clear();
            
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
                    // attack code
                    AddComponent(self, new HitBox(owner, default));
                    GetComponentData<Position>(self).Value       = position + new Vector3(0, 1, 0);
                    GetComponentData<UnitPlayState>(self)        = GetComponentData<UnitPlayState>(owner);
                    GetComponentData<HitBoxAgainstEnemies>(self) = new HitBoxAgainstEnemies(GetComponentData<Relative<TeamDescription>>(owner).Target);
                }
            }
            else if (state.IsChaining)
                controlVelocity.StayAtCurrentPositionX(50);

            var (enemyPrioritySelf, dist) = GetNearestEnemy(owner.Handle, 2, null);
            if (state.IsActive && enemyPrioritySelf != default)
            {
                var targetPosition = GetComponentData<Position>(enemyPrioritySelf).Value;
                if (ability.AttackStart == default)
                    controlVelocity.SetAbsolutePositionX(targetPosition.X, 50);

                // We should have a mercy in the distance of where the unit is and where it should throw. (it shouldn't be able to only throw at a perfect position)
                const float distanceMercy = 2f;
                // If we're near enough of where we should throw the spear, throw it.
                if (MathF.Abs(targetPosition.X - position.X) < distanceMercy && ability.TriggerAttack(worldTime.ToStruct()))
                {
                }

                controlVelocity.OffsetFactor = 0;
            }
        }

        protected override void OnSetup(GameEntity self)
        {
        }
    }
}
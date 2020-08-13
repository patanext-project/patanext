using System;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using PataNext.Simulation.Mixed.Abilities.Subset;
using PataNext.Simulation.mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase.Roles.Components;

namespace PataNext.Simulation.Mixed.Abilities.CTate
{
	public struct TaterazayEnergyFieldAbility : IComponentData
	{
		public float MinDistance, MaxDistance;

		public float GivenDamageReduction, GivenDefenseReal;

		//public GameEntity BuffEntity;

		public class Register : RegisterGameHostComponentData<TaterazayEnergyFieldAbility>
		{
		}
	}

	public class TaterazayEnergyFieldAbilityProvider : BaseRhythmAbilityProvider<TaterazayEnergyFieldAbility>
	{
		protected override string FilePathPrefix => "tate";

		public TaterazayEnergyFieldAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		public override string MasterServerId => "CTate.EnergyField";

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<DefendCommand>();
		}

		public override ComponentType[] GetHeroModeAllowedCommands()
		{
			return new[] {GameWorld.AsComponentType<MarchCommand>()};
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			base.GetComponents(entityComponents);

			entityComponents.Add(GameWorld.AsComponentType<DefaultSubsetMarch>());
		}

		public override void SetEntityData(GameEntity entity, CreateAbility data)
		{
			base.SetEntityData(entity, data);

			GameWorld.GetComponentData<AbilityActivation>(entity).Type = EAbilityActivationType.HeroMode;
			GameWorld.GetComponentData<DefaultSubsetMarch>(entity) = new DefaultSubsetMarch
			{
				Target             = DefaultSubsetMarch.ETarget.Cursor,
				AccelerationFactor = 1
			};

		}
	}

	public class TaterazayEnergyFieldAbilitySystem : BaseAbilitySystem
	{
		private IManagedWorldTime worldTime;
		public TaterazayEnergyFieldAbilitySystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery abilityQuery;

		public override void OnAbilityUpdate()
		{
			var marchCommandComponentType = AsComponentType<MarchCommand>();
			
			var abilityAccessor         = new ComponentDataAccessor<TaterazayEnergyFieldAbility>(GameWorld);
			var subsetAccessor          = new ComponentDataAccessor<DefaultSubsetMarch>(GameWorld);
			var stateAccessor           = new ComponentDataAccessor<AbilityState>(GameWorld);
			var engineSetAccessor       = new ComponentDataAccessor<AbilityEngineSet>(GameWorld);
			var ownerAccessor           = new ComponentDataAccessor<Owner>(GameWorld);
			var controlVelocityAccessor = new ComponentDataAccessor<AbilityControlVelocity>(GameWorld);
			
			var positionAccessor = new ComponentDataAccessor<Position>(GameWorld);
			foreach (var entity in (abilityQuery ??= CreateEntityQuery(new[]
			{
				AsComponentType<TaterazayEnergyFieldAbility>(),
				AsComponentType<DefaultSubsetMarch>(),
				AsComponentType<AbilityState>(),
				AsComponentType<AbilityEngineSet>(),
				AsComponentType<Owner>(),
				AsComponentType<AbilityControlVelocity>()
			})).GetEntities())
			{
				// TODO: Remove buff if owner is invalid
				
				ref readonly var owner = ref ownerAccessor[entity].Target;
				ref readonly var state = ref stateAccessor[entity];
				ref readonly var engineSet = ref engineSetAccessor[entity];
				
				ref var control = ref controlVelocityAccessor[entity];
				ref var subset = ref subsetAccessor[entity];
				subset.IsActive = (state.Phase & EAbilityPhase.Active) != 0 && HasComponent(engineSet.Command.Entity, marchCommandComponentType);

				if (TryGetComponentData(owner, out Relative<UnitTargetDescription> relativeTarget))
				{
					ref readonly var targetPosition = ref positionAccessor[relativeTarget.Target].Value;
					if ((state.Phase & EAbilityPhase.ActiveOrChaining) != 0)
					{
						control.IsActive       = true;
						control.TargetPosition = targetPosition;
						control.Acceleration   = 25;
					}
				}
				
				// TODO: Set Buff here
			}
		}
	}
}
using System;
using System.Collections.Generic;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Native;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.BaseSystems
{
	public struct CreateAbility
	{
		public GameEntity       Owner     { get; set; }
		public AbilitySelection Selection { get; set; }
	}

	public abstract class BaseRhythmAbilityProvider : BaseProvider<CreateAbility>
	{
		public abstract string MasterServerId { get; }

		private AbilityCollectionSystem abilityCollectionSystem;

		protected LocalRhythmCommandResourceManager localRhythmCommandResourceManager;

		protected BaseRhythmAbilityProvider(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref abilityCollectionSystem);
			DependencyResolver.Add(() => ref localRhythmCommandResourceManager);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			abilityCollectionSystem.Register(this);
		}

		public Dictionary<string, object> DataMap;

		/// <summary>
		/// Get the chaining command (tail) of this ability
		/// </summary>
		/// <returns>The chaining command</returns>
		public abstract ComponentType GetChainingCommand();

		/// <summary>
		/// Get the combo commands that would be used before <see cref="GetChainingCommand"/>
		/// </summary>
		/// <returns></returns>
		public virtual ComponentType[] GetComboCommands() => Array.Empty<ComponentType>();

		/// <summary>
		/// Get the commands that are allowed in HeroMode.
		/// </summary>
		/// <returns></returns>
		public virtual ComponentType[] GetHeroModeAllowedCommands() => Array.Empty<ComponentType>();
	}

	public abstract class BaseRhythmAbilityProvider<TAbility> : BaseRhythmAbilityProvider
		where TAbility : struct, IEntityComponent
	{
		protected BaseRhythmAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			entityComponents.AddRange(stackalloc ComponentType[]
			{
				GameWorld.AsComponentType<AbilityDescription>(),
				GameWorld.AsComponentType<TAbility>(),
				GameWorld.AsComponentType<Owner>()
			});

			entityComponents.AddRange(stackalloc ComponentType[]
			{
				GameWorld.AsComponentType<AbilityState>(),
				GameWorld.AsComponentType<AbilityEngineSet>(),
				GameWorld.AsComponentType<AbilityActivation>(),
				GameWorld.AsComponentType<AbilityCommands>(),
			});
		}

		public override void SetEntityData(GameEntity entity, CreateAbility data)
		{
			var combos = new FixedBuffer32<GameEntity>();

			var commandDb = localRhythmCommandResourceManager.DataBase;
			GameWorld.GetComponentData<AbilityCommands>(entity) = new AbilityCommands
			{
				Chaining = commandDb.GetOrCreate(GetChainingCommand())
			};

			GameWorld.GetComponentData<Owner>(entity) = new Owner(data.Owner);
			GameWorld.SetLinkedTo(entity, data.Owner, true);
		}
	}
}
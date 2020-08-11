using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Native;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Game.Abilities;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
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
		/// <summary>
		/// Get whether or not this ability modify automatically the Unit PlayState based on rhythm actions
		/// </summary>
		public virtual bool UseStatsModification => true;

		public class GetAbilityDescStorageStrategy : IDependencyStrategy
		{
			private GameHostModule                                           module;
			private DependencyResolver.ReturnByRefDependency<GameHostModule> dep;

			public GetAbilityDescStorageStrategy(AppSystem appSystem)
			{
				dep = new DependencyResolver.ReturnByRefDependency<GameHostModule>(typeof(GameHostModule),
					() => ref module,
					new DefaultAppObjectStrategy(appSystem, appSystem.World));
			}

			public object ResolveNow(Type type)
			{
				dep.Resolve();
				if (!dep.IsResolved)
					return null;

				if (module == null)
					throw new InvalidOperationException("Invalid result");

				if (module is IModuleHasAbilityDescStorage hasStorage)
					return hasStorage.Value;

				throw new InvalidOperationException($"Module {module.GetType()} does not implement 'IModuleHasAbilityDescStorage'");
			}

			public Func<object> GetResolver(Type type)
			{
				return () => ResolveNow(type);
			}
		}

		public abstract string MasterServerId { get; }

		private AbilityCollectionSystem abilityCollectionSystem;

		protected LocalRhythmCommandResourceManager localRhythmCommandResourceManager;
		protected AbilityDescStorage                abilityStorage;

		protected BaseRhythmAbilityProvider(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref abilityCollectionSystem);
			DependencyResolver.Add(() => ref localRhythmCommandResourceManager);
			DependencyResolver.Add(() => ref abilityStorage, new GetAbilityDescStorageStrategy(this));
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

		protected string configuration;

		public string GetConfigurationData()
		{
			return configuration;
		}
	}

	public abstract class BaseRhythmAbilityProvider<TAbility> : BaseRhythmAbilityProvider
		where TAbility : struct, IEntityComponent
	{
		protected virtual string FilePathPrefix => string.Empty;

		protected virtual string FilePath
		{
			get
			{
				string getShortComponentName(ComponentType componentType)
				{
					var name = GameWorld.Boards.ComponentType.NameColumns[(int) componentType.Id];
					return name.Substring(name.LastIndexOf(':') + 1);
				}

				var folder = "{0}";
				if (!string.IsNullOrEmpty(FilePathPrefix))
					folder = string.Format(folder, FilePathPrefix + "/{0}/");

				var comboCommands = GetComboCommands();
				if (comboCommands == null || comboCommands.Length == 0)
					folder = string.Format(folder, getShortComponentName(GetChainingCommand()));
				else
				{
					folder = string.Format(folder, string.Join("_", comboCommands.Append(GetChainingCommand()).Select(getShortComponentName)));
				}

				// kinda useless, but it will automatically create folders that don't exist, so it's a bit useful for lazy persons (eg: guerro)
				try
				{
					abilityStorage.GetOrCreateDirectoryAsync(folder);
				}
				catch
				{
					// ignored (DllStorage will throw an exception if it does not exist)
				}

				return $"{folder}/{typeof(TAbility).Name.Replace("Ability", string.Empty)}.json";
			}
		}

		protected BaseRhythmAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			foreach (var file in abilityStorage.GetFilesAsync(FilePath).Result)
			{
				configuration = Encoding.UTF8.GetString(file.GetContentAsync().Result);
				break;
			}
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

			if (UseStatsModification)
				entityComponents.AddRange(stackalloc ComponentType[]
				{
					GameWorld.AsComponentType<AbilityModifyStatsOnChaining>()
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

			if (UseStatsModification)
			{
				ref var component = ref GameWorld.GetComponentData<AbilityModifyStatsOnChaining>(entity);

				var stats = new Dictionary<string, StatisticModifier>();
				StatisticModifierJson.FromMap(ref stats, GetConfigurationData());

				void TryGet(string val, out StatisticModifier modifier)
				{
					if (!stats.TryGetValue(val, out modifier))
						modifier = StatisticModifier.Default;
				}

				TryGet("active", out component.ActiveModifier);
				TryGet("fever", out component.FeverModifier);
				TryGet("perfect", out component.PerfectModifier);
				TryGet("charge", out component.ChargeModifier);
			}
		}
	}
}
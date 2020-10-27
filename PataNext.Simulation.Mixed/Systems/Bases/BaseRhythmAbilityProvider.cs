using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.Core.Modules.Feature;
using GameHost.Injection;
using GameHost.Native;
using GameHost.Native.Fixed;
using GameHost.Simulation.Application;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using Microsoft.Extensions.Logging;
using PataNext.Game.Abilities;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using PataNext.Module.Simulation.Systems;
using PataNext.Simulation.Mixed.Components.GamePlay.Abilities;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.BaseSystems
{
	public struct CreateAbility
	{
		public GameEntity       Owner     { get; set; }
		public AbilitySelection Selection { get; set; }
	}

	public struct Resources
	{
		public string HeroModeActivationSound;
	}

	[RestrictToApplication(typeof(SimulationApplication))]
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

			public override string ToString()
			{
				return $"GeAbilityDescStorage({(module == null ? "null" : $"{module.Source.Get<RegisteredModule>().Description.NameId}")})";
			}
		}

		public abstract string    MasterServerId { get; }
		public abstract Resources Resources      { get; }

		private AbilityCollectionSystem abilityCollectionSystem;

		protected LocalRhythmCommandResourceManager localRhythmCommandResourceManager;
		protected AbilityDescStorage                abilityStorage;
		protected ILogger                           logger;

		protected BaseRhythmAbilityProvider(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref abilityCollectionSystem);
			DependencyResolver.Add(() => ref localRhythmCommandResourceManager);
			DependencyResolver.Add(() => ref abilityStorage, new GetAbilityDescStorageStrategy(this));
			DependencyResolver.Add(() => ref logger);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			abilityCollectionSystem.Register(this);
		}

		public string ProvidedJson;

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

		protected virtual string FolderPath
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
					folder = string.Format(folder, FilePathPrefix + "\\{0}");

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

				return folder;
			}
		}
		
		protected virtual string FilePath => $"{FolderPath}\\{typeof(TAbility).Name.Replace("Ability", string.Empty)}";

		protected BaseRhythmAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		private         Resources resources;
		public override Resources Resources => resources;

		private static ReadOnlySpan<byte> Utf8Bom => new byte[] {0xEF, 0xBB, 0xBF};

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			
			foreach (var file in abilityStorage.GetFilesAsync(FilePath + ".json").Result)
			{
				configuration = Encoding.UTF8.GetString(file.GetContentAsync().Result);
				break;
			}

			if (!string.IsNullOrEmpty(configuration))
			{
				using var stream   = new MemoryStream(Encoding.UTF8.GetBytes(configuration));
				using var document = JsonDocument.Parse(stream);
				if (document.RootElement.TryGetProperty("resources", out var prop))
				{
					if (prop.TryGetProperty(nameof(Resources.HeroModeActivationSound), out var heroModeProp))
						resources.HeroModeActivationSound = heroModeProp.GetString();
				}
				if (document.RootElement.TryGetProperty("config", out prop))
				{
					ReadConfiguration(prop);
				}
			}
		}

		protected virtual void ReadConfiguration(JsonElement jsonElement)
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

			if (UseStatsModification)
				entityComponents.AddRange(stackalloc ComponentType[]
				{
					GameWorld.AsComponentType<AbilityModifyStatsOnChaining>(),
					GameWorld.AsComponentType<AbilityControlVelocity>()
				});

			if (MasterServerId != null)
				entityComponents.AddRange(stackalloc ComponentType[]
				{
					GameWorld.AsComponentType<NamedAbilityId>()
				});
		}

		public override void SetEntityData(GameEntity entity, CreateAbility data)
		{
			var combos = new FixedBuffer32<GameEntity>();

			var commandDb = localRhythmCommandResourceManager.DataBase;
			GameWorld.GetComponentData<AbilityCommands>(entity) = new AbilityCommands
			{
				Chaining = commandDb.GetOrCreate(GetChainingCommand()),
			};
			GameWorld.GetComponentData<AbilityActivation>(entity) = new AbilityActivation
			{
				Selection                                = data.Selection,
				HeroModeMaxCombo                         = -1,
				HeroModeImperfectLimitBeforeDeactivation = 2
			};

			var allowedHeroModeCommands = GetHeroModeAllowedCommands();
			if (allowedHeroModeCommands != null)
			{
				ref var buffer = ref GameWorld.GetComponentData<AbilityCommands>(entity).HeroModeAllowedCommands;
				foreach (var cmd in allowedHeroModeCommands)
					buffer.Add(commandDb.GetOrCreate(cmd));
			}

			GameWorld.GetComponentData<Owner>(entity) = new Owner(data.Owner);
			GameWorld.SetLinkedTo(entity, data.Owner, true);

			if (MasterServerId != null)
				GameWorld.GetComponentData<NamedAbilityId>(entity) = new NamedAbilityId(MasterServerId);

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
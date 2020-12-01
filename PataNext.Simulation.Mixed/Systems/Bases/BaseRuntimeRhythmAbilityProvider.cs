using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Worlds;
using PataNext.Game;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Simulation.mixed.Components.GamePlay.Abilities;
using StormiumTeam.GameBase;
using ZLogger;

namespace PataNext.Module.Simulation.BaseSystems
{
	public interface IRuntimeAbilityProvider
	{
		AbilityScript  CurrentScriptObject   { get; }
		Bindable<bool> IsLoadingScriptObject { get; }
		bool           IsScriptDisposable    { get; }

		void SetScriptObject(AbilityScript script, bool disposeAtNextSet = true);
	}

	public abstract class BaseRuntimeRhythmAbilityProvider<T> : BaseRhythmAbilityProvider<T>, IRuntimeAbilityProvider
		where T : struct, IComponentData
	{
		private IScheduler  scheduler;
		private GlobalWorld globalWorld;

		private GameEntity global;

		public AbilityScript  CurrentScriptObject   { get; private set; }
		public Bindable<bool> IsLoadingScriptObject { get; }
		public bool           IsScriptDisposable    { get; private set; }

		protected virtual AbilityScript CreateDefaultScriptObject()
		{
			return null;
		}

		protected BaseRuntimeRhythmAbilityProvider(WorldCollection collection) : base(collection)
		{
			IsLoadingScriptObject = new Bindable<bool>();

			DependencyResolver.Add(() => ref scheduler);
			DependencyResolver.Add(() => ref globalWorld);
		}

		private ScriptRunner   scriptRunner;
		private StorageWatcher watcher;

		private void SetupWarning(GameEntity self)
		{
			//logger.ZLogWarning("Setup function should have been replaced!");
		}

		private void ExecuteWarning(GameEntity owner, GameEntity self, ref AbilityState state)
		{
			//logger.ZLogWarning("Execute function should have been replaced!");
		}

		private SetupExecutableAbility.Func setupWarning;
		private ExecutableAbility.Func      executeWarning;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			global = GameWorld.Safe(GameWorld.CreateEntity());
			GameWorld.AddComponent(global.Handle, new SetupExecutableAbility(setupWarning ??= SetupWarning));
			GameWorld.AddComponent(global.Handle, new ExecutableAbility(executeWarning    ??= ExecuteWarning));

			var def = CreateDefaultScriptObject();
			if (CurrentScriptObject is not null
			    && def is not null)
			{
				SetScriptObject(def);
			}

			AddDisposable(scriptRunner = new CSharpScriptRunner());
			AddDisposable(watcher      = new StorageWatcher(Path.GetFileName(FilePath + ".cs")));
			watcher.Add(abilityStorage.GetOrCreateDirectoryAsync(FolderPath).Result);
			watcher.OnAnyUpdate += (sender, args) =>
			{
				Console.WriteLine(args.ChangeType);
				if ((args.ChangeType & WatcherChangeTypes.Created) != 0 || (args.ChangeType & WatcherChangeTypes.Changed) != 0)
				{
					globalWorld.Scheduler.Schedule(fullPath => { LoadScript(File.ReadAllBytes(fullPath)); }, args.FullPath, default);
				}
			};

			abilityStorage.GetFilesAsync(FilePath + ".cs").ContinueWith(t =>
			{
				var result = t.Result.FirstOrDefault();
				if (result is not null)
					LoadScript(result.GetContentAsync().Result);
			}, TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		public override void SetEntityData(GameEntityHandle entity, CreateAbility data)
		{
			base.SetEntityData(entity, data);

			GameWorld.AssignComponent(entity, GameWorld.GetComponentReference<ExecutableAbility>(global.Handle));
		}

		private void LoadScript(byte[] data)
		{
			scheduler.Schedule(() =>
			{
				IsLoadingScriptObject.Value = true;
				scriptRunner.SetAndLoad(data);
				IsLoadingScriptObject.Value = false;
			}, default);
		}

		public void SetScriptObject(AbilityScript script, bool disposeAtNextSet = true)
		{
			Console.WriteLine("Set Script Object");
			if (CurrentScriptObject != null && IsScriptDisposable)
			{
				if (CurrentScriptObject is IDisposable disposable)
					disposable.Dispose();

				if (CurrentScriptObject is IWorldSystem worldSystem
				    && World.DefaultSystemCollection.SystemList.Contains(worldSystem))
				{
					World.Remove(worldSystem);
				}
			}

			IsScriptDisposable = disposeAtNextSet;

			GameWorld.UpdateOwnedComponent(global.Handle, new SetupExecutableAbility(setupWarning ??= SetupWarning));
			GameWorld.UpdateOwnedComponent(global.Handle, new ExecutableAbility(executeWarning    ??= ExecuteWarning));

			CurrentScriptObject = script;
			if (CurrentScriptObject != null)
			{
				CurrentScriptObject.DependencyResolver
				                   .AsTask
				                   .ContinueWith(t => { scheduler.Schedule(() => { CurrentScriptObject.Init(global); }, default); });
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Passes;
using PataNext.Simulation.mixed.Components.GamePlay.Abilities;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.BaseSystems
{
	[RestrictToApplication(typeof(SimulationApplication))]
	public abstract class AbilityScript : GameAppSystem
	{
		protected GameEntity lastGlobal;

		protected AbilityScript(WorldCollection collection) : base(collection)
		{
		}

		private SetupExecutableAbility.Func setup;
		private ExecutableAbility.Func      execute;

		internal void Init(GameEntity global)
		{
			GameWorld.UpdateOwnedComponent(global.Handle, new SetupExecutableAbility(setup ??= Setup));
			GameWorld.UpdateOwnedComponent(global.Handle, new ExecutableAbility(execute    ??= Execute));

			lastGlobal = global;
		}

		private void Setup(GameEntity self)
		{
			if (DependencyResolver.Dependencies.Count > 0)
				return;
			
			// No this is not an error.
			// We take ExecutableAbility and not SetupExecutableAbility.
			// If we did, this mean the "child" entities need to also have a reference to SetupExecutableAbility.
			// But this would mean the setup would be executed multiple time.
			// It could be possible to fix that to force the Setup to only be executed by the owner of the component.
			//
			// We also start the span from index 1 to ignore the global entity.
			OnSetup(GameWorld.GetReferencedEntities(GameWorld.GetComponentReference<ExecutableAbility>(self.Handle))[1..]);
		}

		private void Execute(GameEntity owner, GameEntity self, ref AbilityState state)
		{
			if (DependencyResolver.Dependencies.Count > 0)
				return;
			OnExecute(owner, self, ref state);
		}

		protected abstract void OnSetup(Span<GameEntityHandle> abilities);
		protected abstract void OnExecute(GameEntity           owner, GameEntity self, ref AbilityState state);

		public override void Dispose()
		{
			base.Dispose();

			setup   = null;
			execute = null;
		}
	}

	public abstract class AbilityScriptModule<TProvider> : AbilityScript
		where TProvider : AppObject, IRuntimeAbilityProvider
	{
		private TProvider provider;

		protected AbilityScriptModule(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref provider);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			AddDisposable(provider.IsLoadingScriptObject.Subscribe((previous, next) =>
			{
				if (next || provider.CurrentScriptObject != null)
					return;

				provider.SetScriptObject(this, disposeAtNextSet: false);
			}, true));
		}

		public override void Dispose()
		{
			if (!provider.IsDisposed && provider.CurrentScriptObject == this)
				provider.SetScriptObject(null);

			base.Dispose();
		}
	}
}
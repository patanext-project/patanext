using System.Collections.Generic;
using System.Linq;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Passes;
using PataNext.Simulation.mixed.Components.GamePlay.Abilities;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.BaseSystems
{
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
			GameWorld.UpdateOwnedComponent(global, new SetupExecutableAbility(setup ??= Setup));
			GameWorld.UpdateOwnedComponent(global, new ExecutableAbility(execute    ??= Execute));

			lastGlobal = global;
		}

		private void Setup(GameEntity self)
		{
			if (DependencyResolver.Dependencies.Count > 0)
				return;
			OnSetup(self);
		}

		private void Execute(GameEntity owner, GameEntity self, AbilityState state)
		{
			if (DependencyResolver.Dependencies.Count > 0)
				return;
			OnExecute(owner, self, state);
		}

		protected abstract void OnSetup(GameEntity   self);
		protected abstract void OnExecute(GameEntity owner, GameEntity self, AbilityState state);
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

			provider.IsLoadingScriptObject.Subscribe((previous, next) =>
			{
				if (next || provider.CurrentScriptObject != null)
					return;

				provider.SetScriptObject(this, disposeAtNextSet: false);
			}, true);
		}

		public override void Dispose()
		{
			if (!provider.IsDisposed && provider.CurrentScriptObject == this)
				provider.SetScriptObject(null);

			base.Dispose();
		}
	}
}
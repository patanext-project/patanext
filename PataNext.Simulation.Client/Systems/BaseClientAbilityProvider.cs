using System;
using System.Collections.Generic;
using GameHost.Core.Ecs;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Simulation.Client.Systems;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Game.Abilities
{
	public class BaseClientAbilityProvider<TServerProvider> : GameAppSystem
		where TServerProvider : BaseRhythmAbilityProvider
	{
		private AbilityHeroVoiceManager heroVoiceManager;

		protected TServerProvider ServerProvider;

		public BaseClientAbilityProvider(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref heroVoiceManager);

			DependencyResolver.Add(() => ref ServerProvider);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			var resources = ServerProvider.Resources;
			Console.WriteLine($"Resource.HeroModeActivationSound={resources.HeroModeActivationSound}");
			if (!string.IsNullOrEmpty(resources.HeroModeActivationSound))
			{
				heroVoiceManager.Register(ServerProvider.MasterServerId, resources.HeroModeActivationSound);
			}
		}
	}
}
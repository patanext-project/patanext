using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.GamePlay.Damage
{
	public class DestroyPreviousDamageEventSystem : GameAppSystem, IGameEventPass
	{
		public DestroyPreviousDamageEventSystem([NotNull] WorldCollection collection) : base(collection)
		{
		}

		private EntityQuery query;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			//query = CreateEntityQuery(new[] {typeof()});
		}
		

		public void OnGameEventPass()
		{
			
		}
	}
}
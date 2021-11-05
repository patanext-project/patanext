using System.Collections.Generic;
using GameHost.Core.Ecs;
using PataNext.Game;

namespace PataNext.CoreMissions.Mixed
{
	public abstract class RegisterMissionSystemBase : AppSystem
	{
		protected MissionManager MissionManager;
		
		public RegisterMissionSystemBase(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref MissionManager);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			
			Register();
		}

		protected abstract void Register();
	}
}
using System;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.Modding.Components;

namespace PataponGameHost.Systems
{
	[RestrictToApplication(typeof(MainThreadHost))]
	public class LoadModuleOnSight : AppSystem
	{
		public LoadModuleOnSight(WorldCollection collection) : base(collection)
		{
			World.Mgr.SubscribeComponentAdded(new ComponentAddedHandler<RegisteredModule>(onModuleRegistered));
		}

		private void onModuleRegistered(in Entity entity, in RegisteredModule value)
		{
			if (value.State != ModuleState.None)
				return;

			Console.WriteLine($"A module in view! {value.Info.DisplayName}");
			World.Mgr.CreateEntity()
			     .Set(new RequestLoadModule {Module = entity});
		}
	}
}
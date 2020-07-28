using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Inputs.Interfaces;
using GameHost.Inputs.Systems;

namespace PataNext.Export.Desktop.Visual.Systems
{
	[RestrictToApplication(typeof(IOsuFrameworkApplication))]
	public class OsuInputBackendRegisterActionSystem : AppSystem
	{
		private InputActionSystemGroup actionSystemGroup;
		
		public OsuInputBackendRegisterActionSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref actionSystemGroup);
		}
		
		public Entity TryGetCreateActionBase(string ghType)
		{
			InputActionSystemBase system;
			if ((system = actionSystemGroup.GetSystemOrDefault(ghType)) != null)
			{
				return system.CreateEntityAction();
			}

			return default;
		}
	}
}
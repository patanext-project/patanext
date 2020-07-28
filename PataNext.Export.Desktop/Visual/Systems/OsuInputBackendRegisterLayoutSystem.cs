using System;
using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Inputs.Layouts;
using GameHost.Inputs.Systems;
using osu.Framework.Logging;

namespace PataNext.Export.Desktop.Visual.Systems
{
	[RestrictToApplication(typeof(IOsuFrameworkApplication))]
	public class OsuInputBackendRegisterLayoutSystem : AppSystem
	{
		private InputActionSystemGroup actionSystemGroup;

		public OsuInputBackendRegisterLayoutSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref actionSystemGroup);
		}

		public InputLayoutBase TryCreateLayout(string ghType, string layoutId)
		{
			InputActionSystemBase system;
			if ((system = actionSystemGroup.GetSystemOrDefault(ghType)) != null)
			{
				return system.CreateLayout(layoutId);
			}

			return null;
		}
	}
}
using System;
using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Threading.Apps;
using GameHost.Worlds;

namespace PataponGameHost
{
	public class SimulationApplication : CommonApplicationThreadListener
	{
		public SimulationApplication(GlobalWorld source, Context overrideContext) : base(source, overrideContext)
		{
			var systems = new List<Type>();
			AppSystemResolver.ResolveFor<SimulationApplication>(systems);
			
			foreach (var type in systems)
			{
				source.Collection.GetOrCreate(type);
			}
		}
	}
}
using System;
using System.Collections.Generic;
using DefaultEcs;
using DefaultEcs.Command;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Core.Threading;
using GameHost.Inputs.Layouts;
using GameHost.Simulation.Application;
using GameHost.Worlds;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(SimulationApplication))]
	public class AddInputSimulationFeature : AppSystem
	{
		public AddInputSimulationFeature(WorldCollection collection) : base(collection)
		{
			collection.Mgr
			          .CreateEntity()
			          .Set(new InputCurrentLayout());
		}
	}
}
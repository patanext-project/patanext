using System;
using DefaultEcs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;
using PataNext.Game;
using StormiumTeam.GameBase;

[assembly: RegisterAvailableModule("PN Core Missions 'Mixed'", "guerro", typeof(PataNext.CoreMissions.Mixed.Module))]

namespace PataNext.CoreMissions.Mixed
{
	public class Module : GameHostModule
	{
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
			var global = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();

			foreach (ref readonly var listener in global.World.Get<IListener>())
			{
				if (listener is SimulationApplication simulationApplication)
				{
					simulationApplication.Schedule(() =>
					{
						var missionMgr = new ContextBindingStrategy(simulationApplication.Data.Context, true).Resolve<MissionManager>();

						missionMgr.Register(new(ResPath.EType.ClientResource, "st", "pn", "mission/test"), "TestMission", new(ResPath.EType.ClientResource, "guerro", "test", "scenar"));
					}, default);
				}
			}
		}
	}
}
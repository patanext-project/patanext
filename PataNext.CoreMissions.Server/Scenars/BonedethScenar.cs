using System;
using System.Threading.Tasks;
using GameHost.Core.Ecs;
using PataNext.CoreMissions.Mixed;
using PataNext.CoreMissions.Mixed.Missions;
using PataNext.Game.Scenar;
using PataNext.Module.Simulation.Game.Scenar;
using StormiumTeam.GameBase;

namespace PataNext.CoreMissions.Server.Scenars
{
	public class BonedethScenar : ScenarScript
	{
		public class Provider : ScenarProvider
		{
			public Provider(WorldCollection collection) : base(collection)
			{
			}

			public override ResPath ScenarPath => BonedethMissionRegister.ScenarPath;

			public override IScenar Provide()
			{
				return new BonedethScenar(World);
			}
		}

		public BonedethScenar(WorldCollection wc) : base(wc)
		{
		}

		protected override async Task OnStart()
		{
		}

		protected override async Task OnLoop()
		{
		}

		protected override async Task OnCleanup(bool reuse)
		{
		}
	}
}
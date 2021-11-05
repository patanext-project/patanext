using System;
using System.Numerics;
using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.CoreAbilities.Mixed.CYari;
using PataNext.CoreMissions.Mixed.Missions;
using PataNext.CoreMissions.Server.Game;
using PataNext.CoreMissions.Server.Providers;
using PataNext.Game.Scenar;
using PataNext.Module.Simulation.Components.GamePlay.Special.Squad;
using PataNext.Module.Simulation.Components.GamePlay.Structures.Bastion;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Providers;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreMissions.Server.Scenars
{
	public class DebugScenar : MissionScenarScript
	{
		public DebugScenar(WorldCollection wc) : base(wc)
		{
		}
		
		protected override Task OnStart()
		{
			base.OnStart();
			
			using var query = CreateEntityQuery(new[] { typeof(UnitDescription) });
			foreach (var entity in query)
				CreateAbility(entity, new[] { "ability", "taterazay", "super" });

			return Task.CompletedTask;
		}

		protected override async Task OnLoop()
		{
		}

		protected override async Task OnCleanup(bool reuse)
		{
		}

		public class Provider : ScenarProvider
		{
			public Provider(WorldCollection collection) : base(collection)
			{
			}

			public override ResPath ScenarPath => DebugMissionRegister.ScenarPath;

			public override IScenar Provide()
			{
				return new DebugScenar(World);
			}
		}
	}
}
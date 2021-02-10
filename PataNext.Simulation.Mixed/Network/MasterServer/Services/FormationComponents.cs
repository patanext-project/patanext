using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using JetBrains.Annotations;
using PataNext.MasterServerShared.Services;
using StormiumTeam.GameBase.Network.MasterServer;

namespace PataNext.Module.Simulation.Network.MasterServer.Services
{
	public struct GetFormationRequest
	{
		public string SaveId;

		public struct Response
		{
			public CurrentSaveFormation Result;
		}

		public class Process : MasterServerRequestHub<IFormationHub, IFormationReceiver, GetFormationRequest>
		{
			public Process([NotNull] WorldCollection collection) : base(collection)
			{
			}

			protected override async Task OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				Debug.Assert(Service != null, "Service != null");
				
				var result = await Service.GetFormation(entity.Get<GetFormationRequest>().SaveId);
				entity.Set(new Response {Result = result});
			}
		}
	}
}
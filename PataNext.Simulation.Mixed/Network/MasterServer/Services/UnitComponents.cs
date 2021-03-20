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
	public struct GetUnitDetailsRequest
	{
		public string UnitId;

		public GetUnitDetailsRequest(string unitId) => UnitId = unitId;

		public struct Response
		{
			public UnitInformation Result;
		}

		public class Process : MasterServerRequestHub<IUnitHub, IUnitHubReceiver, GetUnitDetailsRequest>
		{
			public Process([NotNull] WorldCollection collection) : base(collection)
			{
			}

			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				Debug.Assert(Service != null, "Service != null");

				var result = await Service.GetDetails(entity.Get<GetUnitDetailsRequest>().UnitId);
				return e => e.Set(new Response {Result = result});
			}
		}
	}
}
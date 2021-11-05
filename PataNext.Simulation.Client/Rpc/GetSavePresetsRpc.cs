using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Injection;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Utility;
using PataNext.Game.Rpc.SerializationUtility;
using PataNext.Module.Simulation.Components.Network;
using PataNext.Module.Simulation.Network.MasterServer.Services;
using StormiumTeam.GameBase.Network.MasterServer.Utility;
using StormiumTeam.GameBase.Roles.Descriptions;

namespace PataNext.Simulation.Client.Rpc
{
	public struct GetSavePresetsRpc : IGameHostRpcWithResponsePacket<GetSavePresetsRpc.Response>
	{
		public MasterServerSaveId Save;

		public struct Response : IGameHostRpcResponsePacket
		{
			public struct Preset
			{
				public MasterServerUnitPresetId Id;
				public string                   Name;
				public string                   ArchetypeId;
				public string                   KitId;
				public string                   RoleId;
			}

			public Preset[] Presets;
		}

		public class Process : RpcPacketWithResponseSystem<GetSavePresetsRpc, Response>
		{
			public Process(WorldCollection collection) : base(collection)
			{
			}

			public override string MethodName => "PataNext.GetPresets";

			protected override async ValueTask<Response> GetResponse(GetSavePresetsRpc request)
			{
				Console.WriteLine("!");
				
				var app = GetClientAppUtility.Get(this);
				return await app.TaskScheduler.StartUnwrap(async () =>
				{
					if (string.IsNullOrEmpty(request.Save.Value))
					{
						if (GetClientAppUtility.GetLocalPlayerSave(app) is { } localPlayerSave)
							request.Save = new(localPlayerSave);
						else
							return await WithError(1, "couldn't resolve local save");
					}

					Console.WriteLine("?");
					
					var masterRequest = RequestUtility.New(app.Data.World, new GetSaveUnitPresetsRequest(request.Save.Value));
					var response      = await masterRequest.GetAsync<GetSaveUnitPresetsRequest.Response>();

					var array = new Response.Preset[response.PresetIds.Length];
					var tasks = new Task<GetUnitPresetDetailsRequest.Response>[array.Length];

					for (var i = 0; i < tasks.Length; i++)
						tasks[i] = RequestUtility.New(app.Data.World, new GetUnitPresetDetailsRequest(response.PresetIds[i]))
						                         .GetAsync<GetUnitPresetDetailsRequest.Response>();

					for (var i = 0; i < tasks.Length; i++)
					{
						var details = (await tasks[i]).Result;
						array[i] = new()
						{
							Id          = new(response.PresetIds[i]),
							Name        = details.CustomName,
							KitId       = details.KitId,
							ArchetypeId = details.ArchetypeId,
							RoleId      = details.RoleId
						};
					}

					return await WithResult(new() {Presets = array});
				});
			}
		}
	}
}
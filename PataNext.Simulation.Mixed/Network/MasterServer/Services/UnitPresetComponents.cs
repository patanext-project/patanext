using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using JetBrains.Annotations;
using PataNext.MasterServerShared;
using PataNext.MasterServerShared.Services;
using StormiumTeam.GameBase.Network.MasterServer;

namespace PataNext.Module.Simulation.Network.MasterServer.Services
{
	public struct GetUnitPresetDetailsRequest
	{
		public string PresetId;

		public GetUnitPresetDetailsRequest(string presetId) => PresetId = presetId;

		public struct Response
		{
			public UnitPresetInformation Result;
		}

		public class Process : MasterServerRequestHub<IUnitPresetHub, IUnitPresetHubReceiver, GetUnitPresetDetailsRequest>
		{
			public Process([NotNull] WorldCollection collection) : base(collection)
			{
			}

			protected override async Task OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				Debug.Assert(Service != null, "Service != null");

				var result = await Service.GetDetails(entity.Get<GetUnitPresetDetailsRequest>().PresetId);
				entity.Set(new Response {Result = result});
			}
		}
	}

	public struct GetUnitPresetEquipmentsRequest
	{
		public string PresetId;

		public GetUnitPresetEquipmentsRequest(string presetId) => PresetId = presetId;

		public struct Response
		{
			/// <summary>
			/// 
			/// </summary>
			/// <remarks>
			///	Key=RootId
			/// Value=ItemId
			/// </remarks>
			public Dictionary<string, string> Result;
		}

		public class Process : MasterServerRequestHub<IUnitPresetHub, IUnitPresetHubReceiver, GetUnitPresetEquipmentsRequest>
		{
			public Process([NotNull] WorldCollection collection) : base(collection)
			{
			}

			protected override async Task OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				Debug.Assert(Service != null, "Service != null");

				var result = await Service.GetEquipments(entity.Get<GetUnitPresetEquipmentsRequest>().PresetId);
				entity.Set(new Response {Result = result});
			}
		}
	}

	public struct GetUnitPresetAbilitiesRequest
	{
		public string PresetId;

		public GetUnitPresetAbilitiesRequest(string presetId) => PresetId = presetId;

		public struct Response
		{
			/// <summary>
			/// 
			/// </summary>
			/// <remarks>
			///	Key=ProfileId
			/// Value=Dictionary Of `AbilityComboView In `SongAssetId
			/// </remarks>
			public Dictionary<string, Dictionary<string, MessageComboAbilityView>> Result;
		}

		public class Process : MasterServerRequestHub<IUnitPresetHub, IUnitPresetHubReceiver, GetUnitPresetAbilitiesRequest>
		{
			public Process([NotNull] WorldCollection collection) : base(collection)
			{
			}

			protected override async Task OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				Debug.Assert(Service != null, "Service != null");

				var result = await Service.GetAbilities(entity.Get<GetUnitPresetAbilitiesRequest>().PresetId);
				entity.Set(new Response {Result = result});
			}
		}
	}
}
using System;
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
	public struct GetSaveUnitPresetsRequest
	{
		public string SaveId;

		public GetSaveUnitPresetsRequest(string saveId) => SaveId = saveId;

		public struct Response
		{
			public string[] PresetIds;
		}

		public class Process : MasterServerRequestHub<IUnitPresetHub, IUnitPresetHubReceiver, GetSaveUnitPresetsRequest>
		{
			public Process([NotNull] WorldCollection collection) : base(collection)
			{
			}

			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				Debug.Assert(Service != null, "Service != null");

				var result = await Service.GetSoftPresets(entity.Get<GetSaveUnitPresetsRequest>().SaveId);
				return e => e.Set(new Response { PresetIds = result });
			}
		}
	}

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

			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				Debug.Assert(Service != null, "Service != null");

				var result = await Service.GetDetails(entity.Get<GetUnitPresetDetailsRequest>().PresetId);
				return e => e.Set(new Response { Result = result });
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

			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				Debug.Assert(Service != null, "Service != null");

				var result = await Service.GetEquipments(entity.Get<GetUnitPresetEquipmentsRequest>().PresetId);
				return e => e.Set(new Response { Result = result });
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

			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				Debug.Assert(Service != null, "Service != null");

				var result = await Service.GetAbilities(entity.Get<GetUnitPresetAbilitiesRequest>().PresetId);
				return e => e.Set(new Response { Result = result });
			}
		}
	}

	public struct CopyPresetToTargetUnitRequest
	{
		public string SoftPresetId;
		public string UnitId;

		public CopyPresetToTargetUnitRequest(string softPresetId, string unitId)
		{
			SoftPresetId = softPresetId;
			UnitId       = unitId;
		}

		public class Process : MasterServerRequestHub<IUnitPresetHub, IUnitPresetHubReceiver, CopyPresetToTargetUnitRequest>
		{
			public Process([NotNull] WorldCollection collection) : base(collection)
			{
			}

			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				Debug.Assert(Service != null, "Service != null");

				var req = entity.Get<CopyPresetToTargetUnitRequest>();
				Console.WriteLine($"yooooo {req.SoftPresetId} {req.UnitId}");

				await Service.CopyPresetToTargetUnit(req.SoftPresetId, req.UnitId);

				Console.WriteLine("copied!");

				return _ => { };
			}
		}
	}

	public struct SetPresetEquipments
	{
		public string                     PresetId;
		public Dictionary<string, string> Updates;

		public SetPresetEquipments(string presetId, Dictionary<string, string> updates)
		{
			PresetId = presetId;
			Updates  = updates;
		}

		public SetPresetEquipments(string presetId, string attachmentId, string itemId)
		{
			PresetId = presetId;
			Updates = new()
			{
				{ attachmentId, itemId }
			};
		}

		public class Process : MasterServerRequestHub<IUnitPresetHub, IUnitPresetHubReceiver, SetPresetEquipments>
		{
			public Process([NotNull] WorldCollection collection) : base(collection)
			{
			}

			protected override async Task<Action<Entity>> OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				Debug.Assert(Service != null, "Service != null");

				var req = entity.Get<SetPresetEquipments>();
				Console.WriteLine($"modifying equipment of preset {req.PresetId} {req.Updates.Count}");

				await Service.SetEquipments(req.PresetId, req.Updates);

				Console.WriteLine("modified");

				return _ => { };
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Injection;
using GameHost.Utility;
using PataNext.Game;
using PataNext.Game.GameItems;
using PataNext.Game.Rpc.SerializationUtility;
using PataNext.Module.Simulation.Components;
using StormiumTeam.GameBase;

namespace PataNext.Simulation.Client.Rpc
{
	public struct GetDentComponentsRpc : IGameHostRpcWithResponsePacket<GetDentComponentsRpc.Response>
	{
		public SerializableEntity Dent;

		public struct Response : IGameHostRpcResponsePacket
		{
			public Dictionary<string, string> ComponentTypeToJson;
		}

		public class Process : RpcPacketWithResponseSystem<GetDentComponentsRpc, Response>
		{
			public Process(WorldCollection collection) : base(collection)
			{
			}

			public override string MethodName => "PataNext.GetDentComponents";

			protected override async ValueTask<Response> GetResponse(GetDentComponentsRpc request)
			{
				var app = GetClientAppUtility.Get(this);
				return await app.TaskScheduler.StartUnwrap(async () =>
				{
					var entity = (Entity)request.Dent;
					if (entity.IsAlive == false)
						return await WithError(1, "Entity doesn't exist");

					Response response;
					response.ComponentTypeToJson = new();

					if (entity.TryGet(out ItemInventory itemInventory))
					{
						Component_ItemDetails component;

						var assetEntity = itemInventory.AssetEntity;
						var desc        = assetEntity.Get<GameItemDescription>();
						if (assetEntity.TryGet(out EquipmentItemDescription equipmentItemDescription))
							component.ItemType = equipmentItemDescription.ItemType;
						else
							component.ItemType = string.Empty;

						component.Asset = desc.Id;

						component.Name        = desc.Name;
						component.Description = desc.Description;
						component.AssetType   = desc.Type;

						response.ComponentTypeToJson["PataNext.Core.ItemDetails"] = JsonSerializer.Serialize(component, new JsonSerializerOptions { IncludeFields = true });
					}

					if (entity.TryGet(out MissionDetails missionDetails))
					{
						Component_MissionDetails component;
						component.Path   = missionDetails.Path;
						component.Name   = missionDetails.Name;
						component.Scenar = missionDetails.Scenar;

						response.ComponentTypeToJson["PataNext.Core.MissionDetails"] = JsonSerializer.Serialize(component, new JsonSerializerOptions { IncludeFields = true });
					}

					return await WithResult(response);
				});
			}
		}
	}

	public struct Component_ItemDetails
	{
		public ResPath Asset;
		public string  AssetType;
		public string  Name;
		public string  Description;

		public string ItemType;
	}

	public struct Component_MissionDetails
	{
		public ResPath Path;
		public ResPath Scenar;
		public string  Name;
	}
}
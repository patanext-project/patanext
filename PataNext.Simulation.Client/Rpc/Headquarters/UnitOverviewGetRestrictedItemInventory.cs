using System;
using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Simulation.TabEcs;
using GameHost.Utility;
using PataNext.Game.Rpc.SerializationUtility;
using StormiumTeam.GameBase;

namespace PataNext.Simulation.Client.Rpc
{
	public struct UnitOverviewGetRestrictedItemInventory : IGameHostRpcWithResponsePacket<UnitOverviewGetRestrictedItemInventory.Response>
	{
		public GameEntity EntityTarget;
		public string     AttachmentTarget;

		public struct Response : IGameHostRpcResponsePacket
		{
			public struct Item
			{
				public MasterServerItemId Id;
				public ResPath            AssetResPath;

				public string             AssetType;
				public string             Name;
				public string             Description;
				public MasterServerUnitId EquippedBy;
			}

			public Item[] Items;
		}

		/*public class Process : RpcPacketWithResponseSystem<UnitOverviewGetRestrictedItemInventory, Response>
		{
			public Process(WorldCollection collection) : base(collection)
			{
			}

			public override string MethodName => "PataNext.Client.UnitOverview.GetRestrictedItemInventory";
			protected override async ValueTask<Response> GetResponse(UnitOverviewGetRestrictedItemInventory request)
			{
				var app = GetClientAppUtility.Get(this);

				Console.WriteLine("inventory wanted!");

				return await app.TaskScheduler.StartUnwrap(async () =>
				{
					if (string.IsNullOrEmpty(request.Save.Value))
					{
						if (GetClientAppUtility.GetLocalPlayerSave(app) is { } localPlayerSave)
							request.Save = new(localPlayerSave);
						else
							return await WithError(1, "couldn't resolve local save");
					}

					if (request.Save.Equals(default))
						return await WithError(1, "couldn't resolve local save");
				});
			}
		}*/
	}
}
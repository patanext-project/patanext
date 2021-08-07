using System;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Simulation.Application;
using GameHost.Utility;
using PataNext.Module.Simulation.Network.MasterServer.Services;
using StormiumTeam.GameBase.Network.MasterServer;
using StormiumTeam.GameBase.Network.MasterServer.AssetService;
using StormiumTeam.GameBase.Network.MasterServer.StandardAuthService;
using StormiumTeam.GameBase.Network.MasterServer.User;
using StormiumTeam.GameBase.Network.MasterServer.UserService;
using StormiumTeam.GameBase.Network.MasterServer.Utility;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(SimulationApplication))]
	[DontInjectSystemToWorld]
	public class AddMasterServerFeature : AppSystem
	{
		private TaskScheduler taskScheduler;

		public AddMasterServerFeature(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref taskScheduler);
			
			AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
			
			collection.Mgr.CreateEntity().Set<IFeature>(new MasterServerFeature("http://82.65.166.186:5000"));

			World.Mgr.CreateEntity().Set(new DisconnectUserRequest("12345689"));
			World.Mgr.CreateEntity().Set(ConnectUserRequest.ViaLogin("guerro323", "1316050985"));
			
			return;
			
			World.Mgr.SubscribeComponentChanged((in Entity e, in CurrentUser prev, in CurrentUser curr) =>
			{
				if (string.IsNullOrEmpty(curr.Value.Token))
					return;


				/*taskScheduler.StartUnwrap(async () =>
				{
					for (var i = 0; i < 1; i++)
					{
						var request  = RequestUtility.New(World.Mgr, new GetInventoryRequest(new[] {"equipment"}));
						var response = await request.GetAsync<GetInventoryRequest.Response>();

						var str = "ITEMS:";
						foreach (var id in response.ItemIds)
						{
							var itemDetailsRequest = RequestUtility.New(World.Mgr, new GetItemDetailsRequest(id));
							var detailsResponse    = await itemDetailsRequest.GetAsync<GetItemDetailsRequest.Response>();
							
							str += $"\n\t{id} ({detailsResponse.ResPath}, {detailsResponse.Type}) --> {detailsResponse.Name}";
						}

						Console.WriteLine(str);
					}
				});*/

				/*RequestUtility.CreateTracked(World.Mgr, new ListGameSaveRequest(), (Entity _, ListGameSaveRequest.Response response) =>
				{
					foreach (var saveId in response.Results)
					{
						Console.WriteLine($"saveId={saveId}");

						RequestUtility.CreateFireAndForget(World.Mgr, new SetFavoriteGameSaveRequest(saveId));
					}
				});

				RequestUtility.CreateTracked(World.Mgr, new GetFavoriteGameSaveRequest(), (Entity _, GetFavoriteGameSaveRequest.Response response) =>
				{
					Console.WriteLine($"Favorite game save of {response.UserGuid} = {response.SaveId}");

					RequestUtility.CreateTracked(World.Mgr, new GetFormationRequest {SaveId = response.SaveId}, (Entity _, GetFormationRequest.Response response) =>
					{
						var str = "Current formation:\n";
						str += $"  FlagBearer={response.Result.FlagBearer}\n";
						str += $"  UberHero={response.Result.UberHero}\n";
						for (var i = 0; i < response.Result.Squads.Length; i++)
						{
							str += $"  Squad {i}\n";
							str += $"     Leader={response.Result.Squads[i].Leader}\n";
							str += $"     Soldiers ({response.Result.Squads[i].Soldiers.Length})\n";
						}

						Console.WriteLine(str);
						
						var ts = World.Ctx.Container.GetOrDefault<TaskScheduler>();
						ts.StartUnwrap(async () =>
						{
							var uhStat = "UberHero Stat : \n";

							var (_, unitDetails) =  await RequestUtility.CreateAsync<GetUnitDetailsRequest, GetUnitDetailsRequest.Response>(World.Mgr, new() {UnitId = response.Result.UberHero});
							uhStat               += $"  SaveId: {unitDetails.Result.SaveId}\n";
							uhStat               += $"  HardPresetId: {unitDetails.Result.HardPresetId}\n";
							uhStat               += $"  SoftPresetId: {unitDetails.Result.SoftPresetId}\n\n";

							var (_, presetDetails) =  await RequestUtility.CreateAsync<GetUnitPresetDetailsRequest, GetUnitPresetDetailsRequest.Response>(World.Mgr, new() {PresetId = unitDetails.Result.HardPresetId});
							uhStat                 += $"  Kit: {(await RequestUtility.New(World.Mgr, new GetAssetPointerRequest(presetDetails.Result.KitId)).GetAsync<GetAssetPointerRequest.Response>()).ResPath.FullString}\n";
							uhStat                 += $"  Arch: {(await RequestUtility.New(World.Mgr, new GetAssetPointerRequest(presetDetails.Result.ArchetypeId)).GetAsync<GetAssetPointerRequest.Response>()).ResPath.FullString}\n";

							var (_, equipments) =  await RequestUtility.CreateAsync<GetUnitPresetEquipmentsRequest, GetUnitPresetEquipmentsRequest.Response>(World.Mgr, new() {PresetId = unitDetails.Result.HardPresetId});
							uhStat              += $"  Equipments\n";
							foreach (var (key, value) in equipments.Result)
							{
								var ka = await RequestUtility.New(World.Mgr, new GetAssetPointerRequest(key)).GetAsync<GetAssetPointerRequest.Response>();
								var va = await RequestUtility.New(World.Mgr, new GetItemAssetPointerRequest(value)).GetAsync<GetItemAssetPointerRequest.Response>();
								
								uhStat += $"    {ka.ResPath.FullString}: {va.ResPath.FullString}\n";
							}

							Console.WriteLine(uhStat);
						});
						
						RequestUtility.CreateTracked(World.Mgr, new GetUnitDetailsRequest {UnitId = response.Result.UberHero}, (Entity _, GetUnitDetailsRequest.Response response) =>
						{

							RequestUtility.CreateTracked(World.Mgr, new GetUnitPresetDetailsRequest {PresetId = response.Result.HardPresetId}, (Entity _, GetUnitPresetDetailsRequest response) =>
							{

							});
						});
					});
				});*/
			});
		}
	}
}
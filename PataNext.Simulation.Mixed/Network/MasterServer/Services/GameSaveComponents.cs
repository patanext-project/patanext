using System;
using System.Threading;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.MasterServerShared.Services;
using StormiumTeam.GameBase.Network.MasterServer;
using StormiumTeam.GameBase.Network.MasterServer.User;

namespace PataNext.Module.Simulation.Network.MasterServer.Services
{
	public struct CreateGameSaveRequest : IComponentData
	{
		public string Name;

		public CreateGameSaveRequest(string name)
		{
			Name = name;
		}

		public struct Response : IComponentData
		{
			public string SaveId;
		}

		public class Process : MasterServerRequestService<IGameSaveService, CreateGameSaveRequest>
		{
			private CurrentUserSystem currentUserSystem;

			public Process(WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref currentUserSystem);
			}

			protected override async Task OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				var representation = await Service.CreateSave(currentUserSystem.User, entity.Get<CreateGameSaveRequest>().Name);
				entity.Set(new Response {SaveId = representation});
			}
		}
	}

	public struct ListGameSaveRequest
	{
		public struct Response
		{
			public string[] Results;
		}

		public class Process : MasterServerRequestService<IGameSaveService, ListGameSaveRequest>
		{
			private CurrentUserSystem currentUserSystem;

			public Process(WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref currentUserSystem);
			}

			protected override async Task OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				var saveIds = await Service.ListSaves(currentUserSystem.User.Representation);
				entity.Set(new Response {Results = saveIds});
			}
		}
	}

	public struct GetFavoriteGameSaveRequest : IComponentData
	{
		public string UserGuid;

		public GetFavoriteGameSaveRequest(string userGuid)
		{
			UserGuid = userGuid;
		}

		public struct Response : IComponentData
		{
			public string SaveId;
			public string UserGuid;
		}

		public class Process : MasterServerRequestService<IGameSaveService, GetFavoriteGameSaveRequest>
		{
			private CurrentUserSystem currentUserSystem;

			public Process(WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref currentUserSystem);
			}

			protected override async Task OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				var userGuid       = entity.Get<GetFavoriteGameSaveRequest>().UserGuid ?? currentUserSystem.User.Representation ?? string.Empty;
				var representation = await Service.GetFavoriteSave(userGuid);

				try
				{
					entity.Set(new Response
					{
						SaveId   = representation,
						UserGuid = userGuid
					});
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}
	}
	
	public struct SetFavoriteGameSaveRequest : IComponentData
	{
		public string SaveId;

		public SetFavoriteGameSaveRequest(string saveId)
		{
			SaveId = saveId;
		}
		
		public struct Response : IComponentData
		{
		}

		public class Process : MasterServerRequestService<IGameSaveService, SetFavoriteGameSaveRequest>
		{
			private CurrentUserSystem currentUserSystem;

			public Process(WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref currentUserSystem);
			}

			protected override async Task OnUnprocessedRequest(Entity entity, RequestCallerStatus callerStatus)
			{
				var isEmptySave = await Service.SetFavoriteSave(currentUserSystem.User, entity.Get<SetFavoriteGameSaveRequest>().SaveId);
				entity.Set(new Response
				{
				});
			}
		}
	}
}
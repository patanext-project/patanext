using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using Microsoft.Extensions.Logging;
using STMasterServer.Shared.Services;
using ZLogger;

namespace StormiumTeam.GameBase.Network.MasterServer.User
{
	public readonly struct CurrentUser
	{
		public readonly UserToken Value;

		public CurrentUser(UserToken value)
		{
			Value = value;
		}
	}

	public class CurrentUserSystem : AppSystem
	{
		private ILogger logger;
		private Entity  singletonEntity;

		private TaskCompletionSource<UserToken> taskCompletionSource;
		
		public CurrentUserSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref logger);
			singletonEntity = collection.Mgr.CreateEntity();

			collection.Mgr.SetMaxCapacity<CurrentUser>(1);
			singletonEntity.Set(new CurrentUser());

			taskCompletionSource = new TaskCompletionSource<UserToken>();
		}

		public UserToken       User   { get; private set; }
		public Task<UserToken> AsTask => taskCompletionSource.Task;

		public void Set(UserToken userToken)
		{
			logger.ZLogInformation("Current User: {0}", userToken.Representation);

			User = userToken;
			singletonEntity.Set(new CurrentUser(userToken));

			taskCompletionSource.SetResult(userToken);
		}
	}
}
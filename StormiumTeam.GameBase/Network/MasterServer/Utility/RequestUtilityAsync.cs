using System;
using System.Threading.Tasks;
using DefaultEcs;

namespace StormiumTeam.GameBase.Network.MasterServer.Utility
{
	public static partial class RequestUtility
	{
		private static partial class WithResponse<T>
		{
			public static readonly Action<Entity, TaskCompletionSource<(Entity, T)>, T> CreatedTrackedNoDataAsyncCached = (ent, tcs, response) =>
			{
				try
				{
					tcs.SetResult((ent, response));
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					throw;
				}
			};
		}

		public static Task<(Entity entity, TResponse response)> CreateAsync<TRequest, TResponse>(World world, TRequest request)
		{
			var tcs = new TaskCompletionSource<(Entity, TResponse)>();
			CreateTracked(world, request, WithResponse<TResponse>.CreatedTrackedNoDataAsyncCached, tcs, disposeOnCompletion: true);
			return tcs.Task;
		}

		public static Request<TRequest> New<TRequest>(World world, TRequest request)
		{
			return new (world, request);
		}

		public struct Request<T>
		{
			public readonly World World;
			public readonly T     Requested;

			public Request(World world, T requested)
			{
				World     = world;
				Entity    = default;
				Requested = requested;
			}

			public Entity Entity { get; private set; }

			public async Task<TResponse> GetAsync<TResponse>()
			{
				var (entity, response) = await CreateAsync<T, TResponse>(World, Requested);
				Entity                 = entity;
				return response;
			}
		}
	}
}
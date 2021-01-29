using System;
using DefaultEcs;

namespace StormiumTeam.GameBase.Network.MasterServer.Utility
{
	public static class RequestUtility
	{
		public struct DisposableArray : IDisposable
		{
			public readonly IDisposable?[] Disposables;

			public DisposableArray(IDisposable?[] disposables)
			{
				Disposables = disposables;
			}

			public void Dispose()
			{
				foreach (var disposable in Disposables)
					disposable?.Dispose();

				Disposables.AsSpan()
				           .Clear();
			}
		}

		public static Entity CreateFireAndForget<T>(World world, T component)
		{
			var ent = world.CreateEntity();
			ent.Set(component);
			ent.Set(new UntrackedRequest());
			return ent;
		}

		public static (Entity requestEntity, DisposableArray disposable) CreateTracked<TRequest, TResponse>(World world, TRequest request, Action<Entity, TResponse> onCompletion)
		{
			var ent = world.CreateEntity();
			ent.Set(request);

			var disposables = new IDisposable[2];
			disposables[0] = world.SubscribeComponentAdded((in Entity e, in TResponse response) =>
			{
				if (e != ent)
					return;

				onCompletion(e, response);
				foreach (var disposable in disposables)
					disposable.Dispose();

				Array.Clear(disposables, 0, disposables.Length);
			});
			disposables[1] = world.SubscribeComponentChanged((in Entity e, in TResponse _, in TResponse response) =>
			{
				if (e != ent)
					return;

				onCompletion(e, response);
				foreach (var disposable in disposables)
					disposable.Dispose();

				Array.Clear(disposables, 0, disposables.Length);
			});

			return (ent, new DisposableArray(disposables));
		}

		public static DisposableArray UpdateAndTrack<TRequest, TResponse>(Entity entity, TRequest request, Action<Entity, TResponse> onCompletion)
		{
			entity.Set(request);

			var disposables = new IDisposable[2];
			disposables[0] = entity.World.SubscribeComponentAdded((in Entity e, in TResponse response) =>
			{
				if (e != entity)
					return;

				onCompletion(e, response);
				foreach (var disposable in disposables)
					disposable.Dispose();

				Array.Clear(disposables, 0, disposables.Length);
			});
			disposables[1] = entity.World.SubscribeComponentChanged((in Entity e, in TResponse _, in TResponse response) =>
			{
				if (e != entity)
					return;

				onCompletion(e, response);
				foreach (var disposable in disposables)
					disposable.Dispose();

				Array.Clear(disposables, 0, disposables.Length);
			});

			return new DisposableArray(disposables);
		}
	}
}
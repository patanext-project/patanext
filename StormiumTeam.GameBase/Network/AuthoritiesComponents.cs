using System;
using System.Collections.Generic;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Time;
using JetBrains.Annotations;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Utility.Misc;

namespace StormiumTeam.GameBase.Network
{
	/// <summary>
	/// Set the remote authority. If there is no remote, we add the authority to ourselves.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public struct SetRemoteAuthority<T> : IComponentData
	{

	}

	/// <summary>
	/// Force a temporary authority on this entity.
	/// </summary>
	/// <remarks>
	///	If you wish to set a permanent authority, directly set <see cref="TAuthority"/> component on the entity.
	/// </remarks>
	public struct ForceTemporaryAuthority<TAuthority> : IComponentData
	{
		public int SetFrame;
	}

	public class ForceTemporaryAuthoritySystem : GameAppSystem, IPreUpdateSimulationPass
	{
		private Action<(GameEntityHandle, ComponentType)> removeComponent;

		public ForceTemporaryAuthoritySystem([NotNull] WorldCollection collection) : base(collection)
		{
			queryMap = new Dictionary<ComponentType, EntityQuery>();
		}

		private Dictionary<ComponentType, EntityQuery> queryMap;
		private int                                    lastTypeCount;

		private string nameToSearch;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			nameToSearch = TypeExt.GetFriendlyName(typeof(ForceTemporaryAuthority<>));
			nameToSearch = nameToSearch.Remove(nameToSearch.LastIndexOf('<') + 1);

			removeComponent = ((GameEntityHandle ent, ComponentType ct) args) => GameWorld.RemoveComponent(args.ent, args.ct);
		}

		private IScheduler scheduler = new Scheduler();

		public void OnBeforeSimulationUpdate()
		{
			var typeBoard = GameWorld.Boards.ComponentType;
			if (typeBoard.Registered.Length != lastTypeCount)
			{
				foreach (var componentType in typeBoard.Registered.Slice(lastTypeCount))
				{
					var name = typeBoard.NameColumns[(int) componentType.Id];
					if (!name.StartsWith(nameToSearch))
						continue;

					queryMap[componentType] = CreateEntityQuery(new[]
					{
						componentType
					});
				}

				lastTypeCount = typeBoard.Registered.Length;
			}

			if (!GameWorld.TryGetSingleton(out GameTime gameTime))
				return;

			foreach (var (componentType, query) in queryMap)
			{
				var accessor = GetAccessor<int>(componentType);
				foreach (var entity in query)
				{
					ref var frame = ref accessor[entity];
					if (frame == 0)
					{
						frame = gameTime.Frame;
					}
					// add one frame latency, so that we can be sure that the system order don't mess stuff
					// (it's not really a big problem if the authority stay for one more frame TODO: or is it?)
					else if (gameTime.Frame > frame + 1)
					{
						scheduler.Schedule(removeComponent, (entity, componentType), default);
					}
				}
			}

			scheduler.Run();
		}
	}
}
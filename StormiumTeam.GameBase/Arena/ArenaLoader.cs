using System;
using System.Collections.Generic;
using DefaultEcs;
using DefaultEcs.Command;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.IO;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.Arena
{
	public class ArenaLoader : GameAppSystem
	{
		private Dictionary<string, IArenaProvider> providerMap = new();
		
		private EntitySet requestSet;

		public ArenaLoader(WorldCollection collection) : base(collection)
		{
			requestSet = World.Mgr.GetEntities()
			                  .With<ArenaResourceId>()
			                  .With<AskLoadResource<ArenaResource>>()
			                  .AsSet();
		}

		public ResourceHandle<ArenaResource> Load(string resId)
		{
			var request = World.Mgr.CreateEntity();
			request.Set(new ArenaResourceId(resId));
			request.Set(new AskLoadResource<ArenaResource>());

			return new ResourceHandle<ArenaResource>(request);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			using var recorder = new EntityCommandRecorder();

			Span<Entity> requestEntities = stackalloc Entity[requestSet.Count];
			requestSet.GetEntities()
			          .CopyTo(requestEntities);
			
			foreach (ref readonly var entity in requestEntities)
			{
				if (!providerMap.TryGetValue(entity.Get<ArenaResourceId>().Value, out var provider))
					continue;

				provider.RequestLoad();
			}

			recorder.Execute(World.Mgr);
		}

		public void Register(IArenaProvider provider)
		{
			providerMap[provider.ResourceId] = provider;
		}
	}

	public interface IArenaProvider
	{
		string ResourceId { get; }

		void RequestLoad();
	}
}
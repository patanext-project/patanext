using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.IO;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace PataNext.Game.Scenar
{
	public class SearchForScenarSystem : AppSystem
	{
		private EntitySet  requestSet;
		private IScheduler scheduler;

		private ILogger logger;

		public SearchForScenarSystem(WorldCollection collection) : base(collection)
		{
			requestSet = collection.Mgr.GetEntities()
			                       .With<ScenarLoadRequest>()
			                       .AsSet();

			DependencyResolver.Add(() => ref scheduler);
			DependencyResolver.Add(() => ref logger);
		}

		public override bool CanUpdate()
		{
			return base.CanUpdate() && requestSet.Count > 0;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var entities = requestSet.GetEntities()
			                         .ToArray();
			foreach (var entity in entities)
			{
				// TODO: search for files, if no files found, then ask for assemblies

				World.Mgr.Publish(new ScenarRequestAssemblyPassMessage(entity, entity.Get<ScenarLoadRequest>().Path));
				if (!entity.Has<ScenarResource>())
				{
					logger.ZLogWarning("No Scenar found at path " + entity.Get<ScenarLoadRequest>().Path);
					continue;
				}
				else
					entity.Set<IsResourceLoaded<ScenarResource>>();
			}

			requestSet.Remove<ScenarLoadRequest>();
		}
	}
}
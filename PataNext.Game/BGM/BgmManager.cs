using System.Linq;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Simulation.Application;
using NetFabric.Hyperlinq;
using PataNext.Game.BGM;

namespace PataNext.Client.Systems
{
	[RestrictToApplication(typeof(SimulationApplication))]
	public class BgmManager : AppSystem
	{
		private BgmContainerStorage containerStorage;
		private IScheduler          scheduler;

		public BgmManager(WorldCollection worldCollection) : base(worldCollection)
		{
			DependencyResolver.Add(() => ref containerStorage);
			DependencyResolver.Add(() => ref scheduler);
		}

		private Task task;

		private EntitySet bgmSet;
		private EntitySet refreshSet;

		protected override void OnInit()
		{
			base.OnInit();

			bgmSet = World.Mgr.GetEntities()
			              .With<BgmFile>()
			              .AsSet();
			refreshSet = World.Mgr.GetEntities()
			                  .With<RefreshBgmList>()
			                  .AsSet();

			World.Mgr.CreateEntity().Set(new RefreshBgmList());
		}

		public override bool CanUpdate()
		{
			return base.CanUpdate() && task == null && refreshSet.Count > 0;
		}

		protected override async void OnUpdate()
		{
			base.OnUpdate();

			task = Task.Run(DoTask);
			refreshSet.DisposeAllEntities();
		}

		private async void DoTask()
		{
			scheduler.Schedule(bgmSet.DisposeAllEntities, default);

			var fileList = (await containerStorage.GetFilesAsync("*.zip"))
			               .Concat(await containerStorage.GetFilesAsync("*.json"))
			               .ToList();

			foreach (var bgmFile in fileList.Select(f => new BgmFile(f)))
			{
				await bgmFile.ComputeDescription();
				void setOrCreateEntity()
				{
					var ent = bgmSet.GetEntities()
					                .Where(e => e.Get<BgmFile>().Description.Id == bgmFile.Description.Id)
					                .FirstOrDefault();
					if (ent == default)
						ent = World.Mgr.CreateEntity();

					ent.Set(bgmFile);
				}

				scheduler.Schedule(setOrCreateEntity, default);
			}

			void setTaskToNull() => task = null;
			scheduler.Schedule(setTaskToNull, default);
		}
	}

	public struct RefreshBgmList
	{
	}
}
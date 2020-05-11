using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Core.Threading;
using GameHost.Entities;
using GameHost.Injection;
using NetFabric.Hyperlinq;
using PataponGameHost.Storage;
using Enumerable = System.Linq.Enumerable;

namespace PataponGameHost.Applications.MainThread
{
	[RestrictToApplication(typeof(MainThreadHost))]
	public class BgmManager : AppSystem
	{
		private BgmStorage storage;
		private IScheduler scheduler;

		public BgmManager(WorldCollection worldCollection) : base(worldCollection)
		{
			DependencyResolver.Add(() => ref storage);
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
			scheduler.Add(bgmSet.DisposeAllEntities);

			var files = (await storage.GetFilesAsync("*.json")).ToList();
			foreach (var bgmFile in files.Select(f => new BgmFile(f)))
			{
				await bgmFile.ComputeDescription();

				void setOrCreateEntity()
				{
					var ent = bgmSet.GetEntities()
					                .Where(e => e.Get<BgmFile>().Description.Id == bgmFile.Description.Id)
					                .First()
					                .Match(e => e, World.Mgr.CreateEntity);

					ent.Set(bgmFile);
				}

				scheduler.Add(setOrCreateEntity);
			}

			void setTaskToNull() => task = null;
			scheduler.Add(setTaskToNull);
		}
	}

	public struct RefreshBgmList
	{
	}
}
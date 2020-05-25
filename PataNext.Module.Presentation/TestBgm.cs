using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameHost.Applications;
using GameHost.Audio;
using GameHost.Core.Applications;
using GameHost.Core.Audio;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using PataNext.Module.Presentation.BGM;
using PataNext.Module.Presentation.BGM.Directors;
using PataponGameHost.Storage;

namespace PataNext.Module.Presentation
{
	[RestrictToApplication(typeof(GameRenderThreadingHost))]
	public class TestBgm : AppSystem
	{
		private Task                testTask;
		private BgmContainerStorage BgmContainerStorage;
		private LoadAudioResourceSystem loadAudio;
		private IScheduler scheduler;

		public TestBgm(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref BgmContainerStorage);
			DependencyResolver.Add(() => ref loadAudio);
			DependencyResolver.Add(() => ref scheduler);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			testTask = DoTest();
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (testTask.Exception != null)
				throw testTask.Exception;
		}

		private async Task DoTest()
		{
			var bgm = new BgmFile((await BgmContainerStorage.GetFilesAsync("ZippedTheme.zip")).First());
			await bgm.ComputeDescription();
			Console.WriteLine($@"Id={bgm.Description.Id}
Name={bgm.Description.Name}
Author={bgm.Description.Author}
Store={bgm.Description.StorePath}
Path={bgm.FullName}
");

			var director = await BgmDirector.Create(bgm);

			BgmDirectorBase.BCommand command;
			while ((command = director.GetCommand("march")) == null)
				Task.Yield();

			Console.WriteLine($"CommandId={command.Id}, Type={command.GetType()}");
			if (command is BgmDefaultDirector.ComboBasedCommand scoreBasedCommand)
			{
				var cmdFiles = await scoreBasedCommand.PreloadFiles();
				foreach (var f in cmdFiles)
				{
					var audioResource = loadAudio.Start(f);
					while (!audioResource.IsLoaded)
						Task.Yield();

					scheduler.Add(() =>
					{
						var sound = World.Mgr.CreateEntity();
						sound.Set(audioResource.Result);
						sound.Set(new AudioVolumeComponent(1));
						sound.Set(new PlayFlatAudioComponent());
						Console.WriteLine("create audio " + sound);
					});
					
					break;
				}
			}
		}
	}
}
using System;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Bindables;
using GameHost.Core.IO;
using GameHost.Core.Modding;
using GameHost.Core.Threading;
using GameHost.Injection;
using PataNext.Game;
using PataponGameHost;

[assembly: ModuleDescription("PataNext.Game", "guerro", typeof(GameModule))]

namespace PataNext.Game
{
	public class GameModule : CModule
	{
		private readonly IScheduler scheduler;
		private readonly object     bindableProtection;

		public Bindable<IStorage> InputStorage;

		public GameModule(Entity source, Context ctxParent, SModuleInfo original) : base(source, ctxParent, original)
		{
			InputStorage = new Bindable<IStorage>(protection: bindableProtection);
			InputStorage.EnableProtection(true, bindableProtection);

			scheduler = new ContextBindingStrategy(Ctx, true).Resolve<IScheduler>();

			Storage.Subscribe(onStorageUpdate, true);
		}

		private void onStorageUpdate(IStorage previous, IStorage next)
		{
			if (next == null)
				return;

			next.GetOrCreateDirectoryAsync("Inputs").ContinueWith(t =>
			{
				scheduler.AddOnce(() =>
				{
					InputStorage.EnableProtection(false, bindableProtection);
					InputStorage.Value = t.Result;
					InputStorage.EnableProtection(true, bindableProtection);
				});
			});
		}
	}
}
using System;
using GameHost.Game;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;
using PataNext.Export.Desktop.Updater;

namespace PataNext.Export.Desktop.Visual
{
	public class VisualGame : osu.Framework.Game
	{
		private GameBootstrap gameBootstrap;
		private Box           box;

		public VisualGame(GameBootstrap gameBootstrap)
		{
			this.gameBootstrap = gameBootstrap;
		}
		
		private DependencyContainer dependencies;
		
		protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
			dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

		[BackgroundDependencyLoader]
		private void load()
		{
			gameBootstrap.GameEntity.Set(new VisualHWND {Value = Window.WindowInfo.Handle});
			dependencies.Cache(gameBootstrap);

			Child = new Container
			{
				(box = new Box
				{
					Anchor = Anchor.Centre,
					Origin = Anchor.Centre,
					Colour = Color4.Orange,
					Size   = new Vector2(200),
				}),
				new GameHostApplicationRunner(),
				new OsuInputBackend()
			};
		}

		protected override void LoadComplete()
		{
			base.LoadComplete();

			LoadComponentAsync(new SquirrelUpdater(), Add);
			
			box.Loop(b => b.RotateTo(0).RotateTo(360, 2500));
		}

		protected override bool OnExiting()
		{
			gameBootstrap.Dispose();
			return false;
		}
	}
}
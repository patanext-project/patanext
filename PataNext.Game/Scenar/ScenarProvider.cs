using GameHost.Core.Ecs;
using StormiumTeam.GameBase;

namespace PataNext.Game.Scenar
{
	public abstract class ScenarProvider : AppSystem
	{
		public ScenarProvider(WorldCollection collection) : base(collection)
		{
			AddDisposable(collection.Mgr.Subscribe((in ScenarRequestAssemblyPassMessage msg) =>
			{
				if (msg.ScenarPath.Equals(ScenarPath) == false)
					return;

				msg.SetScenar(Provide());
			}));
		}

		public abstract ResPath ScenarPath { get; }

		public abstract IScenar Provide();
	}
}
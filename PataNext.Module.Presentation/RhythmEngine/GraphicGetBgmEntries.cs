using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;

namespace PataNext.Module.Presentation
{
	[RestrictToApplication(typeof(GameRenderThreadingHost))]
	public class GraphicGetBgmEntries : AppSystem
	{
		public GraphicGetBgmEntries(WorldCollection collection) : base(collection)
		{
		}

		[RestrictToApplication(typeof(MainThreadHost))]
		public class RestrictedHost : AppSystem
		{
			public RestrictedHost(WorldCollection collection) : base(collection)
			{
			}
		}
	}
}
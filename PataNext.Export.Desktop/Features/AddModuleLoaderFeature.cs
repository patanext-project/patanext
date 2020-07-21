using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.Modules.Feature;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(ExecutiveEntryApplication))]
	public class AddModuleLoaderFeature : AppSystem
	{
		public AddModuleLoaderFeature(WorldCollection collection) : base(collection)
		{
			collection.Mgr.CreateEntity().Set<IFeature>(new ModuleLoaderFeature());
		}
	}
}
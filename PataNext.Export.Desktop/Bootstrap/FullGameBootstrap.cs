using GameHost.Core.Ecs;
using JetBrains.Annotations;
using StormiumTeam.GameBase.Bootstrap;

namespace PataNext.Export.Desktop.Bootstrap
{
	public class FullGameBootstrap : BootstrapEntry
	{
		public FullGameBootstrap([NotNull] WorldCollection collection) : base(collection)
		{
		}

		protected override void OnExecute(string jsonArgs)
		{
			
		}
	}
}
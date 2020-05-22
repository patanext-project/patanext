using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Modding;
using GameHost.Injection;

namespace PataNext.Module.Presentation
{
	public class CustomModule : CModule
	{
		public CustomModule(Entity source, Context ctxParent, SModuleInfo original) : base(source, ctxParent, original)
		{
			var renderClient = new GameRenderThreadingClient();
			renderClient.Connect();

			renderClient.InjectAssembly(GetType().Assembly);

			var inputClient = new GameInputThreadingClient();
			inputClient.Connect();

			inputClient.InjectAssembly(GetType().Assembly);
		}
	}
}
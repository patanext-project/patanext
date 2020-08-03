using DefaultEcs;
using GameHost.Core.Modules;
using GameHost.Injection;

[assembly: RegisterAvailableModule("GameBase", "StormiumTeam", typeof(StormiumTeam.GameBase.Module))]

namespace StormiumTeam.GameBase
{
	public class Module : GameHostModule
	{
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
		}
	}
}
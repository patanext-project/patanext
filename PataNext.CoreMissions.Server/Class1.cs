using System;
using DefaultEcs;
using GameHost.Core.Modules;
using GameHost.Injection;

[assembly: RegisterAvailableModule("PN Core Missions 'Server'", "guerro", typeof(PataNext.CoreMissions.Server.Module))]

namespace PataNext.CoreMissions.Server
{
	public class Module : GameHostModule
	{
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
		}
	}
}
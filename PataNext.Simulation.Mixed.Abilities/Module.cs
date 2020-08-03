using System;
using DefaultEcs;
using GameHost.Core.Modules;
using GameHost.Injection;

[assembly: RegisterAvailableModule("PataNext Standard Abilities", "guerro", typeof(PataNext.Simulation.Mixed.Abilities.Module))]

namespace PataNext.Simulation.Mixed.Abilities
{
	public class Module : GameHostModule
	{
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
		}
	}
}
using DefaultEcs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;
using PataNext.Game.Abilities;
using PataNext.Simulation.Client.Systems;
using PataNext.Simulation.Client.Systems.Inputs;

namespace PataNext.Simulation.Client
{
	public class Module : GameHostModule
	{
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
			var global = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();
			foreach (var listener in global.World.Get<IListener>())
			{
				if (listener is SimulationApplication simulationApplication)
				{
					simulationApplication.Schedule(() =>
					{
						simulationApplication.Data.Collection.GetOrCreate(typeof(RegisterRhythmEngineInputSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(AbilityHeroVoiceManager));
					}, default);
				}
			}
		}
	}
}
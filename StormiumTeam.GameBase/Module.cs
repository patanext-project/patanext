using DefaultEcs;
using GameHost.Core.Ecs.Passes;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;
using StormiumTeam.GameBase.Time;

[assembly: RegisterAvailableModule("GameBase", "StormiumTeam", typeof(StormiumTeam.GameBase.Module))]

namespace StormiumTeam.GameBase
{
	public class Module : GameHostModule
	{
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
			var global = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();
			foreach (ref readonly var listener in global.World.Get<IListener>())
			{
				if (listener is SimulationApplication simulationApplication)
				{
					var systemCollection = simulationApplication.Data.Collection.DefaultSystemCollection;
					systemCollection.AddPass(new IPreUpdateSimulationPass.RegisterPass(), new[] {typeof(UpdatePassRegister)}, null);
					systemCollection.AddPass(new IUpdateSimulationPass.RegisterPass(), new[] {typeof(IPreUpdateSimulationPass.RegisterPass)}, null);
					systemCollection.AddPass(new IPostUpdateSimulationPass.RegisterPass(), new[] {typeof(IUpdateSimulationPass.RegisterPass)}, null);

					simulationApplication.Data.Collection.GetOrCreate(typeof(SetGameTimeSystem));
				}
			}
		}
	}
}
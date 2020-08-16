using System;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;
using Microsoft.Extensions.Logging;
using PataNext.Module.Simulation;
using PataNext.Module.Simulation.Passes;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Time;

[assembly: RegisterAvailableModule("PataNext Simulation", "guerro", typeof(CustomModule))]

namespace PataNext.Module.Simulation
{
	public class CustomModule : GameHostModule
	{
		public CustomModule(Entity source, Context ctxParent, GameHostModuleDescription original) : base(source, ctxParent, original)
		{
			var logger = (ILogger) new DefaultAppObjectStrategy(this, new WorldCollection(ctxParent, null)).ResolveNow(typeof(ILogger));
			logger.Log(LogLevel.Information, "My custom module has been loaded!");

			var global = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();
			foreach (ref readonly var listener in global.World.Get<IListener>())
			{
				if (listener is SimulationApplication simulationApplication)
				{
					simulationApplication.Data.Collection.DefaultSystemCollection.AddPass(new IRhythmEngineSimulationPass.RegisterPass(),
						new[] {typeof(IUpdateSimulationPass.RegisterPass)},
						new[] {typeof(IPostUpdateSimulationPass.RegisterPass)});

					simulationApplication.Data.Collection.DefaultSystemCollection.AddPass(new IAbilityPreSimulationPass.RegisterPass(),
						new[] {typeof(IUpdateSimulationPass.RegisterPass), typeof(IRhythmEngineSimulationPass.RegisterPass)},
						new[] {typeof(IPostUpdateSimulationPass.RegisterPass)});

					simulationApplication.Data.Collection.DefaultSystemCollection.AddPass(new IAbilitySimulationPass.RegisterPass(),
						new[] {typeof(IUpdateSimulationPass.RegisterPass), typeof(IAbilityPreSimulationPass.RegisterPass)},
						new[] {typeof(IPostUpdateSimulationPass.RegisterPass)});

					simulationApplication.Data.Collection.GetOrCreate(typeof(Systems.LocalRhythmCommandResourceManager));

					simulationApplication.Data.Collection.GetOrCreate(typeof(Game.Providers.PlayableUnitProvider));
					simulationApplication.Data.Collection.GetOrCreate(typeof(Systems.AbilityCollectionSystem));
					simulationApplication.Data.Collection.GetOrCreate(typeof(Components.Roles.AbilityDescription.RegisterContainer));

					simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Units.UnitCalculatePlayStateSystem));

					simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Abilities.UpdateActiveAbilitySystem));
					simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Abilities.ApplyAbilityStatisticOnChainingSystem));

					simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Units.UnitPhysicsSystem));
					simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Abilities.AbilityControlVelocitySystem));

					simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.ManageComponentTagSystem));
					simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.ProcessEngineSystem));
					simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.OnInputForRhythmEngine));
					simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.GetNextCommandEngineSystem));
					simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.ApplyCommandEngineSystem));
					simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.RhythmEngineResizeCommandBufferSystem));

					simulationApplication.Data.Collection.GetOrCreate(typeof(GameModes.YaridaTrainingGameMode));
					simulationApplication.Data.Collection.GetOrCreate(typeof(GameModes.StartYaridaTrainingGameMode));
				}
			}
		}
	}
}
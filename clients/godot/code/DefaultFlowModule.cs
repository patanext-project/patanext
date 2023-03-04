using PataNext.Game.Modules.GameModes.Yarida64;
using PataNext.Module.Abilities.Scripts.Defaults;
using PataNext.Presentations;
using Quadrum.Game.Modules.Simulation.Application;
using revghost;
using revghost.Module;
using revghost.Utility;
using revtask.Core;

namespace PataNext;

public class DefaultFlowModule : HostModule
{    
    public DefaultFlowModule(HostRunnerScope scope) : base(scope)
    {
        HostLogger.Output.Info("Hi!");
    }

    protected override void OnInit()
    {
        LoadModule(sc => new PataNext.Game.Client.Resources.Module(sc));
        LoadModule(sc => new PataNext.Game.Module(sc));
        LoadModule(sc => new PataNext.Game.Client.Module(sc));

        LoadModule(sc => new PataNext.Module.Abilities.Module(sc));

        TrackDomain((SimulationDomain domain) =>
        {
            domain.JobRunner.CompleteBatch(domain.CurrentJob);

            //new RhythmEnginePresentation(domain.Scope);
            new TestUnitPresentationSystem(domain.Scope);
            //new DefaultMarchScript(domain.Scope);
            new Yarida64GameModeSystem(domain.Scope);

            var gm = domain.GameWorld.CreateEntity();
            domain.GameWorld.AddYarida64GameMode(gm, new Yarida64GameMode {YaridaCount = 64});
        });
    }
}
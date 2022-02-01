using PataNext.Export.Godot.Presentation;
using PataNext.Game.Client.RhythmEngineAudio.BGM.Directors;
using PataNext.Game.Client.RhythmEngineAudio.BGM.Directors.Defaults;
using PataNext.Game.Client.RhythmEngineAudio.Systems;
using Quadrum.Game.Modules.Simulation.Application;
using revghost;
using revghost.IO.Storage;
using revghost.Module;
using revghost.Utility;

namespace PataNext.Game.Client;

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        var storage = new MultiStorage
        {
            //ModuleScope.DataStorage, 
            ModuleScope.DllStorage
        };
        
        TrackDomain((SimulationDomain domain) =>
        {
            var scope = new FreeScope(new ChildScopeContext(domain.Scope.Context));
            scope.Context.Register<IStorage>(storage);
            
            Disposables.AddRange(new IDisposable[]
            {
                new UpdatePresentationSystems(domain.Scope),
                new LoadBgmSystem(domain.Scope),
                new OnNewBeatSystem(scope),
                new ShoutDrumSystem(scope),
                
                // TODO: The default director should be its own module
                new BgmDefaultDirectorSoundtrackSystem(scope),
                new BgmDefaultDirectorCommandsSystem(scope)
            });
        });
    }
}
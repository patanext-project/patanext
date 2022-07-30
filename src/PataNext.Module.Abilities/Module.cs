using PataNext.Game.Modules.Abilities.Storages;
using PataNext.Module.Abilities.Providers.Defaults;
using PataNext.Module.Abilities.Scripts.Defaults;
using Quadrum.Game.Modules.Simulation.Application;
using revghost;
using revghost.IO.Storage;
using revghost.Module;
using revghost.Utility;

namespace PataNext.Module.Abilities;

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
        
    }

    protected override void OnInit()
    {
        ModuleScope.Context.Register(new AbilityDescriptionStorage(new MultiStorage
        {
            ModuleScope.DataStorage,
            ModuleScope.DllStorage
        }.GetSubStorage("Descriptions")));
        
        TrackDomain((SimulationDomain domain) =>
        {
            var scope = new FreeScope(new MultipleScopeContext
            {
                ModuleScope.Context,
                domain.Scope.Context
            });
            
            Disposables.AddRange(new IDisposable[]
            {
                new DefaultMarchAbility.Provider(scope),
                new DefaultMarchScript(scope),
                
                new DefaultJumpAbility.Provider(scope),
                new DefaultJumpScript(scope)
            });
        });
    }
}
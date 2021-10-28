using PataNext.Game.Modules.Simulation.Application;
using PataNext.Game.Modules.Simulation.RhythmEngine.Components;
using revghost;
using revghost.Module;

namespace PataNext.Game.Modules.Simulation.RhythmEngine;

public partial class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        TrackDomain((SimulationDomain domain) =>
        {
            domain.SystemGroup.Add(new ApplyTagsSystem());
            domain.SystemGroup.Add(new ResetStateOnStoppedSystem());
            domain.SystemGroup.Add(new ProcessSystem());

            var entity = domain.GameWorld.CreateEntity();
            domain.GameWorld.AddComponent(entity, RhythmEngineLayout.Type.GetOrCreate(domain.GameWorld));

            ref var controller = ref domain.GameWorld.GetComponentData(
                entity,
                RhythmEngineController.Type.GetOrCreate(domain.GameWorld)
            );
            
            controller = controller with
            {
                State = RhythmEngineController.EState.Playing,
                StartTime = domain.WorldTime.Total.Add(TimeSpan.FromSeconds(2))
            };
        });
    }
}
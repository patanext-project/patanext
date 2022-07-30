using System;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using revghost;

namespace PataNext.Export.Desktop;

public class TestSimulationSystem : SimulationSystem
{
    public TestSimulationSystem(Scope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        var a = Simulation.CreateEntity();
        var b = Simulation.CreateEntity();
        
        Simulation.AddComponent(b, AbilityOwnerDescription.Relative.ToComponentType(Simulation), a);

        Console.WriteLine($"owner={Simulation.GetComponentData(b, AbilityOwnerDescription.Relative.Type.GetOrCreate(Simulation))}");
    }
}
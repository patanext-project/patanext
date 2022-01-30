using System.Numerics;
using PataNext.Export.Godot.Presentation;
using Quadrum.Game.Modules.Simulation;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using revecs;
using revecs.Core;
using revecs.Extensions.Generator.Commands;
using revecs.Extensions.Generator.Components;
using revecs.Systems;
using revghost;
using revghost.Module;

namespace PataNext.Export.Godot;

public partial class EntryModule : HostModule
{
    public EntryModule(HostRunnerScope scope) : base(scope)
    {
        Console.WriteLine("EntryModule - .ctor");
    }

    protected override void OnInit()
    {
        // Add Godot related modules here

        Console.WriteLine("EntryModule - OnInit");
        LoadModule(sc => new PataNext.Game.Module(sc));
        LoadModule(sc => new PataNext.Game.Client.Module(sc));

        TrackDomain((SimulationDomain domain) =>
        {
            new RhythmEnginePresentation(domain.Scope);
        });
    }
}
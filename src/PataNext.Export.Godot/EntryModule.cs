using revghost;
using revghost.Module;

namespace PataNext.Export.Godot;

public class EntryModule : HostModule
{
    public EntryModule(HostRunnerScope scope) : base(scope)
    {
        Console.WriteLine("EntryModule - .ctor");
    }

    protected override void OnInit()
    {
        // Add Godot related modules here

        Console.WriteLine("EntryModule - OnInit");
        LoadModule(scope => new PataNext.Game.Module(scope));
    }
}
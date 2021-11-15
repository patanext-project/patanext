using revghost;
using revghost.Module;

namespace PataNext.Game;

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        LoadModule(scope => new Quadrum.Game.Module(scope));
        LoadModule(scope => new Modules.RhythmEngine.Module(scope));
    }
}
using PataNext.Module.Abilities.Providers.Defaults;
using Quadrum.Game.Modules.Simulation.Abilities.Components.Aspects;
using Quadrum.Game.Modules.Simulation.Abilities.SystemBase;
using revecs.Core;
using revecs.Extensions.Generator.Components;
using revghost;

namespace PataNext.Module.Abilities.Scripts.Defaults;

public class DefaultMarchScript : AbilityScript<DefaultMarchAbility>
{
    public DefaultMarchScript(Scope scope) : base(scope)
    {
    }

    protected override void OnSetup(ReadOnlySpan<UEntityHandle> abilities)
    {
    }

    protected override void OnExecute(UEntityHandle owner, UEntityHandle self)
    {
        // The execution part is very simple since we relay it to the aspect system
        // (it's shared between the backward ability, hero mode abilities, etc...)
        Simulation.GetMarchAbilityAspect(self).IsActive = HasActiveState(self);
    }
}
using PataNext.Game.Modules.Abilities.SystemBase;
using PataNext.Game.Modules.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.Abilities.Components.Aspects;
using revecs.Core;
using revecs.Extensions.Generator.Components;
using revghost;

namespace PataNext.Module.Abilities.Providers.Defaults;

public partial struct DefaultMarchAbility : ISparseComponent
{
    public class Provider : BaseRhythmAbilityProvider<DefaultMarchAbility>
    {
        public Provider(Scope scope) : base(scope)
        {
        }

        protected override void GetComboCommands<TList>(TList componentTypes)
        {
            componentTypes.Add(MarchCommand.ToComponentType(Simulation));
        }

        public override UEntityHandle SpawnEntity(CreateAbility data)
        {
            var ability = base.SpawnEntity(data);
            Simulation.AddMarchAbilityAspect(ability, new MarchAbilityAspect
            {
                AccelerationFactor = 1,
                Target = MarchAbilityAspect.ETarget.All
            });

            return ability;
        }
    }
}
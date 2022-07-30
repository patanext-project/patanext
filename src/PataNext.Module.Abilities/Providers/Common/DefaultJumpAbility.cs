using PataNext.Game.Modules.Abilities.SystemBase;
using PataNext.Game.Modules.RhythmEngine.Commands;
using revecs.Core;
using revecs.Extensions.Generator.Components;
using revghost;

namespace PataNext.Module.Abilities.Providers.Defaults;

public partial struct DefaultJumpAbility : ISparseComponent
{
    public int LastActiveId;

    public bool  IsJumping;
    public float ActiveTime;
    
    public class Provider : BaseRhythmAbilityProvider<DefaultJumpAbility>
    {
        public Provider(Scope scope) : base(scope)
        {
        }

        protected override void GetComboCommands<TList>(TList componentTypes)
        {
            componentTypes.Add(JumpCommand.ToComponentType(Simulation));
        }
    }
}
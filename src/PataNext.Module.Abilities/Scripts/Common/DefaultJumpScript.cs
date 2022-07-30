using PataNext.Module.Abilities.Providers.Defaults;
using Quadrum.Game.Modules.Simulation;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Abilities.SystemBase;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Units;
using Quadrum.Game.Utilities;
using revecs.Core;
using revghost;

namespace PataNext.Module.Abilities.Scripts.Defaults;

public partial class DefaultJumpScript : AbilityScript<DefaultJumpAbility>
{
    private const float START_JUMP_TIME = 0.5f;
    
    public DefaultJumpScript(Scope scope) : base(scope)
    {
    }

    private GameTimeQuery timeQuery;
    private SetupCommands setupCommands;
    private ExecuteCommands execCommands;

    protected override void OnInit()
    {
        base.OnInit();

        timeQuery = new GameTimeQuery(Simulation);
        setupCommands = new SetupCommands(Simulation);
        execCommands = new ExecuteCommands(Simulation);
    }

    private float dt; //< delta time

    protected override void OnSetup(ReadOnlySpan<UEntityHandle> abilities)
    {
        dt = (float) timeQuery.First().GameTime.Delta.TotalSeconds;
        
        foreach (var entity in abilities)
        {
            // TODO: HasSimulationAuthority -> continue
            // This piece of code is only executed when the client don't have authority on this ability
            break;
            
            ref var ability = ref setupCommands.UpdateDefaultJumpAbility(entity);
            ref readonly var engineSet = ref setupCommands.ReadAbilityRhythmEngineSet(entity);
            
            var delta = engineSet.State.Elapsed - engineSet.CommandState.StartTime;
            if (HasActiveOrChainingState(entity) && delta > TimeSpan.Zero)
            {
                ability.ActiveTime = (float) delta.TotalSeconds;
                ability.IsJumping  = ability.ActiveTime <= START_JUMP_TIME;
            }
            else
            {
                ability.ActiveTime = 0.0f;
                ability.IsJumping  = false;
            }
        }
    }

    protected override void OnExecute(UEntityHandle owner, UEntityHandle self)
    {
        ref var ability = ref execCommands.UpdateDefaultJumpAbility(self);
        ref readonly var state = ref execCommands.ReadAbilityState(self);
        if (state.ActivationVersion != ability.LastActiveId)
        {
            ability.IsJumping    = false;
            ability.ActiveTime   = 0;
            ability.LastActiveId = state.ActivationVersion;
        }

        ref var velocity = ref execCommands.UpdateVelocityComponent(owner);
        if (!HasActiveOrChainingState(self))
        {
            if (ability.IsJumping)
            {
                velocity.Y = Math.Max(0, velocity.Y - 60 * (ability.ActiveTime * 2));
            }

            ability.ActiveTime = 0;
            ability.IsJumping  = false;
            return;
        }
        
        var wasJumping = ability.IsJumping;
        ability.IsJumping = ability.ActiveTime <= START_JUMP_TIME;

        if (!wasJumping && ability.IsJumping)
            velocity.Y = Math.Max(velocity.Y + 25, 30);
        else if (ability.IsJumping && velocity.Y > 0)
            velocity.Y = Math.Max(velocity.Y - 60 * dt, 0);

        if (ability.ActiveTime < 3.25f)
        {
            ref readonly var playState = ref execCommands.ReadUnitPlayState(owner);
            velocity.X = MathUtils.LerpNormalized(
                velocity.X,
                0,
                dt * (ability.ActiveTime + 1) * Math.Max(0, 1 + playState.Weight * 0.1f)
            );
        }

        if (!ability.IsJumping && velocity.Y > 0)
        {
            velocity.Y = Math.Max(velocity.Y - 10 * dt, 0);
            velocity.Y = MathUtils.LerpNormalized(velocity.Y, 0, 5 * dt);
        }

        ability.ActiveTime += dt;

        ref var unitController = ref execCommands.UpdateUnitControllerState(owner);
        unitController.ControlOverVelocityX = ability.ActiveTime < 3.25f;
        unitController.ControlOverVelocityY = ability.ActiveTime < 2.5f;
    }

    private partial record struct SetupCommands :
        DefaultJumpAbility.Cmd.IWrite,
        AbilityRhythmEngineSet.Cmd.IRead;

    private partial record struct ExecuteCommands :
        DefaultJumpAbility.Cmd.IWrite,
        AbilityState.Cmd.IRead,
        
        UnitControllerState.Cmd.IWrite,
        UnitPlayState.Cmd.IRead,
        VelocityComponent.Cmd.IWrite;
}
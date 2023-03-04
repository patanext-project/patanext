using PataNext.Game.Modules.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs.Core;
using revecs.Extensions.Buffers;
using revecs.Extensions.Generator.Commands;
using revecs.Extensions.Generator.Components;
using revecs.Extensions.LinkedEntity.Generator;
using revecs.Querying;
using revecs.Systems.Generator;
using revghost;

namespace PataNext.Game.Modules.RhythmEngine;

public partial struct DefaultCommandGroup : ITagComponent {}

public partial class CreateCommandSystem : SimulationSystem
{
    public CreateCommandSystem(Scope scope) : base(scope)
    {
        
    }

    protected override void OnInit()
    {
        var cmd = new Commands(Simulation);
        
        var group = cmd.CreateEntity();
        cmd.AddDefaultCommandGroup(group);
        
        SwapStance(cmd, group);
        QuickSwapStance(cmd, group);
        March(cmd, group);
        Attack(cmd, group);
        Defend(cmd, group);
        Jump(cmd, group);
        Party(cmd, group);
        Summon(cmd, group);
        
        static void SwapStance(Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group, "swapstance");
            //cmd.AddSwapStance(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Right));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Right));
            buffer.Add(RhythmCommandAction.With(2, (int) DefaultCommandKeys.Up));
            buffer.Add(RhythmCommandAction.With(3, (int) DefaultCommandKeys.Right));
        }
        
        static void QuickSwapStance(Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group, "swapstance");
            //cmd.AddSwapStance(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Right));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Up));
        }

        static void March(Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group, "march");
            cmd.AddMarchCommand(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(2, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(3, (int) DefaultCommandKeys.Right));
        }
        
        static void Attack(Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group, "attack");
            cmd.AddAttackCommand(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Right));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Right));
            buffer.Add(RhythmCommandAction.With(2, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(3, (int) DefaultCommandKeys.Right));
        }
        
        static void Defend(Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group, "defend");
            cmd.AddDefendCommand(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Up));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Up));
            buffer.Add(RhythmCommandAction.With(2, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(3, (int) DefaultCommandKeys.Right));
        }
        
        static void Jump(Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group, "jump");
            cmd.AddJumpCommand(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Down));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Down));
            buffer.Add(RhythmCommandAction.With(2, (int) DefaultCommandKeys.Up));
            buffer.Add(RhythmCommandAction.With(3, (int) DefaultCommandKeys.Up));
        }
        
        static void Party(Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group, "party");
            cmd.AddPartyCommand(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Right));
            buffer.Add(RhythmCommandAction.With(2, (int) DefaultCommandKeys.Down));
            buffer.Add(RhythmCommandAction.With(3, (int) DefaultCommandKeys.Up));
        }

        static void Summon(Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group, "summon");
            cmd.AddSummonCommand(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Down));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Down));
            buffer.Add(RhythmCommandAction.WithOffset(1, 0.5f, (int) DefaultCommandKeys.Down));
            buffer.Add(RhythmCommandAction.WithOffset(2, 0.5f, (int) DefaultCommandKeys.Down));
            buffer.Add(RhythmCommandAction.WithOffset(3, 0, (int) DefaultCommandKeys.Down));
        }

        static (UEntityHandle output, BufferData<RhythmCommandAction> buffer) Create(Commands cmd, UEntityHandle group, string identifier)
        {
            var (ent, buffer) = CommandFactory.New(cmd);
            cmd.AddEntityLink(ent, group);
            cmd.UpdateCommandDuration(ent).Value = 4;
            
            cmd.AddCommandIdentifier(ent, new CommandIdentifier(identifier));

            return (ent, buffer);
        }
    }

    private partial record struct Commands :
        ICmdEntityAdmin,
        ICmdLinkedEntityAdmin,
        // group
        DefaultCommandGroup.Cmd.IAdmin,
        // command
        CommandIdentifier.Cmd.IAdmin,

        MarchCommand.Cmd.IAdmin,
        AttackCommand.Cmd.IAdmin,
        DefendCommand.Cmd.IAdmin,
        JumpCommand.Cmd.IAdmin,
        PartyCommand.Cmd.IAdmin,
        SummonCommand.Cmd.IAdmin,
        
        // required for factory
        CommandActions.Cmd.IAdmin,
        CommandDuration.Cmd.IAdmin;
}
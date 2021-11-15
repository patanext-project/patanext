using PataNext.Game.Modules.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs;
using revecs.Core;
using revecs.Extensions.Buffers;
using revecs.Extensions.Generator.Commands;
using revecs.Extensions.Generator.Components;
using revecs.Extensions.LinkedEntity.Generator;
using revecs.Systems;

namespace PataNext.Game.Modules.RhythmEngine;

public partial struct CreateCommandSystem : ISystem
{
    private partial struct GroupQuery : IQuery, With<DefaultCommandGroup> {}
    private partial struct Commands : 
        ICmdEntityAdmin, 
        ICmdLinkedEntityAdmin,
        // group
        DefaultCommandGroup.Cmd.IAdmin,
        // command
        MarchCommand.Cmd.IAdmin,
        // required for factory
        CommandActions.Cmd.IAdmin,
        CommandDuration.Cmd.IAdmin
    {}

    [RevolutionSystem]
    private static void Method(
        [Query, Optional] GroupQuery groupQuery,
        [Cmd] Commands cmd
    )
    {
        if (groupQuery.Any())
            return;
        
        var group = cmd.CreateEntity();
        cmd.AddDefaultCommandGroup(group);
        
        March(cmd, group);
        Attack(cmd, group);
        Defend(cmd, group);
        Party(cmd, group);

        static void March(Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group);
            cmd.AddMarchCommand(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(2, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(3, (int) DefaultCommandKeys.Right));
        }
        
        static void Attack(Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group);
            cmd.AddMarchCommand(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Right));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Right));
            buffer.Add(RhythmCommandAction.With(2, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(3, (int) DefaultCommandKeys.Right));
        }
        
        static void Defend(Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group);
            cmd.AddMarchCommand(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Up));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Up));
            buffer.Add(RhythmCommandAction.With(2, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(3, (int) DefaultCommandKeys.Right));
        }
        
        static void Party(Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group);
            cmd.AddMarchCommand(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Right));
            buffer.Add(RhythmCommandAction.With(2, (int) DefaultCommandKeys.Down));
            buffer.Add(RhythmCommandAction.With(3, (int) DefaultCommandKeys.Up));
        }

        static (UEntityHandle output, BufferData<RhythmCommandAction> buffer) Create(Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = CommandFactory.New(cmd);
            cmd.AddEntityLink(ent, group);
            cmd.UpdateCommandDuration(ent).Value = 4;

            return (ent, buffer);
        }
    }

    private partial struct DefaultCommandGroup : ITagComponent {} 
}
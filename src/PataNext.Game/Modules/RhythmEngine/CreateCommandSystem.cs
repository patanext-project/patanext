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
using revtask.Core;

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
        CommandActions.Cmd.IAdmin
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
        March();

        void March()
        {
            var (ent, buffer) = Create();
            cmd.AddMarchCommand(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(2, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(3, (int) DefaultCommandKeys.Right));
        }

        (UEntityHandle output, BufferData<RhythmCommandAction> buffer) Create()
        {
            var (ent, buffer) = CommandFactory.New(cmd);
            cmd.AddEntityLink(ent, group);
            
            return (ent, buffer);
        }
    }

    private partial struct DefaultCommandGroup : ITagComponent {} 
}
using PataNext.Game.Modules.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs.Core;
using revecs.Extensions.Buffers;
using revecs.Extensions.Generator.Commands;
using revecs.Extensions.Generator.Components;
using revecs.Extensions.LinkedEntity.Generator;
using revecs.Systems.Generator;

namespace PataNext.Game.Modules.RhythmEngine;

public partial struct DefaultCommandGroup : ITagComponent {}

public partial struct CreateCommandSystem : IRevolutionSystem,
    ICmdEntityAdmin, 
    ICmdLinkedEntityAdmin,
    // group
    DefaultCommandGroup.Cmd.IAdmin,
    // command
    MarchCommand.Cmd.IAdmin,
    // required for factory
    CommandActions.Cmd.IAdmin,
    CommandDuration.Cmd.IAdmin
{
    public void Constraints(in SystemObject sys)
    {
    }

    public void Body()
    {
        var groupQuery = OptionalQuery(All<DefaultCommandGroup>()); 
        if (groupQuery.Any())
            return;
        
        var group = Cmd.CreateEntity();
        Cmd.AddDefaultCommandGroup(group);
        
        March(Cmd, group);
        Attack(Cmd, group);
        Defend(Cmd, group);
        Party(Cmd, group);

        static void March(__Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group);
            cmd.AddMarchCommand(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(2, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(3, (int) DefaultCommandKeys.Right));
        }
        
        static void Attack(__Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group);
            cmd.AddMarchCommand(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Right));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Right));
            buffer.Add(RhythmCommandAction.With(2, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(3, (int) DefaultCommandKeys.Right));
        }
        
        static void Defend(__Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group);
            cmd.AddMarchCommand(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Up));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Up));
            buffer.Add(RhythmCommandAction.With(2, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(3, (int) DefaultCommandKeys.Right));
        }
        
        static void Party(__Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = Create(cmd, group);
            cmd.AddMarchCommand(ent);
            buffer.Add(RhythmCommandAction.With(0, (int) DefaultCommandKeys.Left));
            buffer.Add(RhythmCommandAction.With(1, (int) DefaultCommandKeys.Right));
            buffer.Add(RhythmCommandAction.With(2, (int) DefaultCommandKeys.Down));
            buffer.Add(RhythmCommandAction.With(3, (int) DefaultCommandKeys.Up));
        }

        static (UEntityHandle output, BufferData<RhythmCommandAction> buffer) Create(__Commands cmd, UEntityHandle group)
        {
            var (ent, buffer) = CommandFactory.New(cmd);
            cmd.AddEntityLink(ent, group);
            cmd.UpdateCommandDuration(ent).Value = 4;

            return (ent, buffer);
        }
    }
}
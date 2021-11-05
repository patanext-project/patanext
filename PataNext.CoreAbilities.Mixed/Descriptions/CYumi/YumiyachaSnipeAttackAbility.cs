using System;
using System.Numerics;
using System.Text.Json;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using Newtonsoft.Json;
using PataNext.CoreAbilities.Mixed.Descriptions;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;

namespace PataNext.CoreAbilities.Mixed.CYumi
{
    public struct YumiyachaSnipeAttackAbility : IThrowProjectileAbility
    {
        public Vector2 ThrowVelocity { get; set; }

        public Vector2 Gravity { get; set; }

        public TimeSpan AttackStart { get; set; }
        public bool     DidAttack   { get; set; }
        public TimeSpan Cooldown    { get; set; }

        public TimeSpan DelayBeforeAttack { get; set; }

        public TimeSpan PauseAfterAttack { get; set; }
    }

    public class YumiyachaSnipeAttackAbilityProvider : BaseRuntimeRhythmAbilityProvider<YumiyachaSnipeAttackAbility>
    {
        public YumiyachaSnipeAttackAbilityProvider(WorldCollection collection) : base(collection)
        {
            DefaultConfiguration = new YumiyachaSnipeAttackAbility
            {
                DelayBeforeAttack = TimeSpan.FromSeconds(0.5f),
                PauseAfterAttack  = TimeSpan.FromSeconds(0.3f),
                ThrowVelocity     = new Vector2(20f, 6),
                Gravity           = new Vector2(0, -15f)
            };
        }

        protected override string FilePathPrefix => "yumi";
        public override    string MasterServerId => resPath.Create(new[] {"ability", "yumi", "snipe_atk"}, ResPath.EType.MasterServer);

        public override ComponentType GetChainingCommand()
        {
            return GameWorld.AsComponentType<AttackCommand>();
        }
    }
}
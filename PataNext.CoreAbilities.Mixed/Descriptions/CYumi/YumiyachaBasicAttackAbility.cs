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
    public struct YumiyachaBasicAttackAbility : IThrowProjectileAbility
    {
        public float AccumulatedAccuracy;
        
        public Vector2 ThrowVelocity { get; set; }

        public Vector2 Gravity { get; set; }

        public TimeSpan AttackStart { get; set; }
        public bool     DidAttack   { get; set; }
        public TimeSpan Cooldown    { get; set; }

        public TimeSpan DelayBeforeAttack { get; set; }

        public TimeSpan PauseAfterAttack { get; set; }
    }

    public class YumiyachaBasicAttackAbilityProvider : BaseRuntimeRhythmAbilityProvider<YumiyachaBasicAttackAbility>
    {
        public YumiyachaBasicAttackAbilityProvider(WorldCollection collection) : base(collection)
        {
            DefaultConfiguration = new YumiyachaBasicAttackAbility
            {
                DelayBeforeAttack = TimeSpan.FromSeconds(0.25),
                PauseAfterAttack = TimeSpan.FromSeconds(0.2f),
                ThrowVelocity    = new Vector2(8f, 15),
                Gravity          = new Vector2(0, -18f)
            };
        }

        protected override string FilePathPrefix => "yumi";
        public override    string MasterServerId => resPath.Create(new[] {"ability", "yumi", "def_atk"}, ResPath.EType.MasterServer);

        public override ComponentType GetChainingCommand()
        {
            return GameWorld.AsComponentType<AttackCommand>();
        }
    }
}
using System;
using System.Numerics;
using System.Text.Json;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using Newtonsoft.Json;
using PataNext.CoreAbilities.Mixed.Descriptions;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;

namespace PataNext.CoreAbilities.Mixed.CYari
{
    public struct YaridaLeapAttackAbility : IThrowProjectileAbilitySettings
    {
        public struct State : SimpleAttackAbility.IState
        {
            public TimeSpan AttackStart { get; set; }
            public bool     DidAttack   { get; set; }
            public TimeSpan Cooldown    { get; set; }
        }

        public TimeSpan DelayBeforeAttack { get; set; }
        public TimeSpan PauseAfterAttack  { get; set; }
        public Vector2  ThrowVelocity     { get; set; }
        public Vector2  Gravity           { get; set; }
    }

    public class YaridaLeapAttackAbilityProvider : BaseRuntimeRhythmAbilityProvider<YaridaLeapAttackAbility>
    {
        public YaridaLeapAttackAbilityProvider(WorldCollection collection) : base(collection)
        {
            DefaultConfiguration = new YaridaLeapAttackAbility
            {
                DelayBeforeAttack = TimeSpan.FromSeconds(0.4f),
                PauseAfterAttack  = TimeSpan.FromSeconds(0.5f),
                ThrowVelocity     = new Vector2(22.5f, -8f),
                Gravity           = new Vector2(0, -26f)
            };
        }

        protected override string FilePathPrefix => "yari";
        public override    string MasterServerId => resPath.GetAbility("yarida", "leap_attack");

        public override void GetComponents(PooledList<ComponentType> entityComponents)
        {
            base.GetComponents(entityComponents);
            
            entityComponents.Add(GameWorld.AsComponentType<YaridaLeapAttackAbility.State>());
        }

        public override ComponentType GetChainingCommand()
        {
            return GameWorld.AsComponentType<AttackCommand>();
        }
    }
}
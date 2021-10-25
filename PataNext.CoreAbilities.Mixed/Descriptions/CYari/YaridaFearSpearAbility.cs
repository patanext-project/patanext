using System;
using System.Numerics;
using System.Text.Json;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using Newtonsoft.Json;
using PataNext.CoreAbilities.Mixed.Descriptions;
using PataNext.CoreAbilities.Mixed.Subset;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;

namespace PataNext.CoreAbilities.Mixed.CYari
{
    public struct YaridaFearSpearAbility : IThrowProjectileAbilitySettings
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

    public class YaridaFearSpearAbilityProvider : BaseRuntimeRhythmAbilityProvider<YaridaFearSpearAbility>
    {
        public YaridaFearSpearAbilityProvider(WorldCollection collection) : base(collection)
        {
            DefaultConfiguration = new YaridaFearSpearAbility
            {
                DelayBeforeAttack = TimeSpan.FromSeconds(0.35f),
                PauseAfterAttack  = TimeSpan.FromSeconds(0.5f),
                ThrowVelocity     = new Vector2(20, -1f),
                Gravity           = new Vector2(0, -28f)
            };
        }

        protected override string FilePathPrefix => "yari";
        public override    string MasterServerId => resPath.GetAbility("yari", "fear_spear");

        public override void GetComponents(PooledList<ComponentType> entityComponents)
        {
            base.GetComponents(entityComponents);
            
            entityComponents.Add(GameWorld.AsComponentType<YaridaFearSpearAbility.State>());
            entityComponents.Add(GameWorld.AsComponentType<DefaultSubsetMarch>());
        }
        
        public override ComponentType[] GetHeroModeAllowedCommands()
        {
            return new[] {GameWorld.AsComponentType<MarchCommand>()};
        }

        public override ComponentType GetChainingCommand()
        {
            return GameWorld.AsComponentType<AttackCommand>();
        }

        public override void SetEntityData(GameEntityHandle entity, CreateAbility data)
        {
            base.SetEntityData(entity, data);

            GameWorld.GetComponentData<AbilityActivation>(entity).Type = EAbilityActivationType.HeroMode | EAbilityActivationType.Alive;
            GameWorld.GetComponentData<DefaultSubsetMarch>(entity) = new DefaultSubsetMarch
            {
                Target             = DefaultSubsetMarch.ETarget.Cursor,
                AccelerationFactor = 1
            };

        }
    }
}
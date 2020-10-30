using System;
using System.Numerics;
using System.Text.Json;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using Newtonsoft.Json;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;

namespace PataNext.CoreAbilities.Mixed.CYari
{
    public struct YaridaBasicAttackAbility : IThrowSpearAbility
    {
        public Vector2 ThrowVelocity { get; set; }

        public Vector2 Gravity { get; set; }

        public TimeSpan AttackStart { get; set; }
        public bool DidAttack { get; set; }
        public TimeSpan Cooldown { get; set; }

        public TimeSpan DelayBeforeAttack { get; set; }

        public TimeSpan PauseAfterAttack { get; set; }
    }

    public class YaridaBasicAttackAbilityProvider : BaseRuntimeRhythmAbilityProvider<YaridaBasicAttackAbility>
    {
        public YaridaBasicAttackAbilityProvider(WorldCollection collection) : base(collection)
        {
        }

        protected override string FilePathPrefix => "yari";
        public override string MasterServerId => resPath.Create(new [] {"ability", "yari", "def_atk"}, ResPath.EType.MasterServer);

        public override ComponentType GetChainingCommand()
        {
            return GameWorld.AsComponentType<AttackCommand>();
        }

        private TimeSpan configThrowDelay = TimeSpan.FromSeconds(0.3);

        protected override void ReadConfiguration(JsonElement jsonElement)
        {
            if (jsonElement.TryGetProperty("throwDelay", out var throwDelayProp))
                configThrowDelay = TimeSpan.FromSeconds(throwDelayProp.GetDouble());
        }

        public override void SetEntityData(GameEntity entity, CreateAbility data)
        {
            base.SetEntityData(entity, data);

            GameWorld.GetComponentData<YaridaBasicAttackAbility>(entity) = new YaridaBasicAttackAbility
            { 
                DelayBeforeAttack = TimeSpan.FromSeconds(Math.Max(
                    configThrowDelay.TotalSeconds, (ProvidedJson == null ? 0 : JsonConvert.DeserializeAnonymousType(ProvidedJson, new {throwDelay = 0.0}).throwDelay)
                )),
                PauseAfterAttack = TimeSpan.FromSeconds(0.5f),
                ThrowVelocity = new Vector2(10, 10),
                Gravity = new Vector2(0, -22.5f)
            };
        }
    }
}
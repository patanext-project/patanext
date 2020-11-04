using System;
using System.Numerics;
using System.Text.Json;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using Newtonsoft.Json;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;

namespace PataNext.CoreAbilities.Mixed.CTate
{
	public struct TaterazayBasicAttackAbility : ISimpleAttackAbility
	{
		public TimeSpan AttackStart       { get; set; }
		public bool     DidAttack         { get; set; }
		public TimeSpan Cooldown          { get; set; }
		public TimeSpan DelayBeforeAttack { get; set; }
		public TimeSpan PauseAfterAttack  { get; set; }
	}

	public class TaterazayBasicAttackAbilityProvider : BaseRuntimeRhythmAbilityProvider<TaterazayBasicAttackAbility>
	{
		public TaterazayBasicAttackAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		protected override string FilePathPrefix => "tate";
		public override    string MasterServerId => resPath.Create(new [] {"ability", "tate", "def_atk"}, ResPath.EType.MasterServer);
		
		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<AttackCommand>();
		}
		
		private TimeSpan attackDelay = TimeSpan.FromSeconds(0.15);

		protected override void ReadConfiguration(JsonElement jsonElement)
		{
			if (jsonElement.TryGetProperty("attackDelay", out var throwDelayProp))
				attackDelay = TimeSpan.FromSeconds(throwDelayProp.GetDouble());
		}

		public override void SetEntityData(GameEntity entity, CreateAbility data)
		{
			base.SetEntityData(entity, data);

			GameWorld.GetComponentData<TaterazayBasicAttackAbility>(entity) = new TaterazayBasicAttackAbility
			{ 
				DelayBeforeAttack = TimeSpan.FromSeconds(Math.Max(
					attackDelay.TotalSeconds, (ProvidedJson == null ? 0 : JsonConvert.DeserializeAnonymousType(ProvidedJson, new {throwDelay = 0.0}).throwDelay)
				)),
				PauseAfterAttack = TimeSpan.FromSeconds(0.5f)
			};
		}
	}
}
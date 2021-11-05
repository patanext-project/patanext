using System;
using System.Numerics;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.CoreAbilities.Mixed.Descriptions;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;

namespace PataNext.CoreAbilities.Mixed.CMega
{
	public struct MegaponBasicMagicAttackAbility : IThrowProjectileAbilitySettings
	{
		public TimeSpan DelayBeforeAttack { get; set; }
		public TimeSpan PauseAfterAttack  { get; set; }
		public int      BurstCount;

		public struct State : SimpleAttackAbility.IState
		{
			public TimeSpan AttackStart { get; set; }
			public bool     DidAttack   { get; set; }
			public TimeSpan Cooldown    { get; set; }
			public int      Burst;
		}

		public Vector2 ThrowVelocity { get; set; }
		public Vector2 Gravity       { get; set; }
	}

	public class MegaponBasicMagicAttackAbilityProvider : BaseRuntimeRhythmAbilityProvider<MegaponBasicMagicAttackAbility>
	{
		public MegaponBasicMagicAttackAbilityProvider(WorldCollection collection) : base(collection)
		{
			DefaultConfiguration = new()
			{
				DelayBeforeAttack = TimeSpan.FromSeconds(0.08),
				PauseAfterAttack  = TimeSpan.FromSeconds(0.01),
				ThrowVelocity     = new Vector2(13, 8f),
				Gravity           = new Vector2(0, -25f),
				BurstCount        = 3
			};
		}

		protected override string FilePathPrefix => "mega";
		public override    string MasterServerId => resPath.GetAbility(MasterServerIdBuilder.MegaponAbility, "magic_atk_def");

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			base.GetComponents(entityComponents);
			
			entityComponents.AddRange(new []
			{
				GameWorld.AsComponentType<MegaponBasicMagicAttackAbility.State>()
			});
		}

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<AttackCommand>();
		}
	}
}
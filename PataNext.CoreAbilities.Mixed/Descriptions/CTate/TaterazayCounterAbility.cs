using System;
using BepuPhysics.Collidables;
using Box2D.NetStandard.Collision.Shapes;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Game.GamePlay.Damage;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay.HitBoxes;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.Physics.Systems;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Mixed.CTate
{
	public struct TaterazayCounterAbility : SimpleAttackAbility.ISettings
	{
		public float  SendBackDamageFactorOnTrigger;
		public float  SendBackDamageFactorAfterTrigger;

		public struct State : SimpleAttackAbility.IState
		{
			public int PreviousActivation;
		
			public double DamageStock;
			
			public TimeSpan AttackStart { get; set; }
			public bool     DidAttack   { get; set; }
			public TimeSpan Cooldown    { get; set; }
		}

		public TimeSpan DelayBeforeAttack { get; set; }
		public TimeSpan PauseAfterAttack  { get; set; }
	}

	public class TaterazayCounterAbilityProvider : BaseRuntimeRhythmAbilityProvider<TaterazayCounterAbility>
	{
		public TaterazayCounterAbilityProvider(WorldCollection collection) : base(collection)
		{
			DefaultConfiguration = new TaterazayCounterAbility
			{
				SendBackDamageFactorOnTrigger    = 1.5f,
				SendBackDamageFactorAfterTrigger = 0.25f,
				DelayBeforeAttack                = TimeSpan.FromSeconds(0.5f),
				PauseAfterAttack                 = TimeSpan.Zero
			};
		}

		protected override string FilePathPrefix => "tate";
		public override    string MasterServerId => resPath.Create(new[] {"ability", "tate", "counter"}, ResPath.EType.MasterServer);

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<DefendCommand>();
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			base.GetComponents(entityComponents);

			// Needed for Hitbox
			entityComponents.AddRange(new[]
			{
				GameWorld.AsComponentType<TaterazayCounterAbility.State>(),
				GameWorld.AsComponentType<Position>(),
				GameWorld.AsComponentType<DamageFrameData>(),
				GameWorld.AsComponentType<HitBoxAgainstEnemies>(),
				GameWorld.AsComponentType<HitBoxHistory>(),
			});
		}
	}
}
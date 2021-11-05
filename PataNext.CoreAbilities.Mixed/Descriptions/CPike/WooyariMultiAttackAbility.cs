using System;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Game.GamePlay.Damage;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay.HitBoxes;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Mixed.CPike
{
	public struct WooyariMultiAttackAbility : ISimpleAttackAbility
	{
		public int         AttackCount;
		public int         PreviousActivation;

		public EAttackType Current;
		public EAttackType Next;

		public ECombo Combo;
		
		public int MaxAttacksPerCommand { get; set; }

		public TimeSpan AttackStart       { get; set; }
		public bool     DidAttack         { get; set; }
		public TimeSpan Cooldown          { get; set; }
		public TimeSpan DelayBeforeAttack { get; set; }
		public TimeSpan PauseAfterAttack  { get; set; }

		public enum EAttackType
		{
			None = 0,
			/// <summary>
			/// Upward slice. 
			/// </summary>
			/// <remarks>
			/// - Slow initial movement speed.
			/// - Keep velocity
			/// - Small jump.
			/// - ++Slash power
			///
			/// Combo with:
			/// -> Uppercut
			///		Allow to get higher with consecutive uppercuts (great for dealing with Toripons)
			///		Nothing else
			/// -> Stab
			///		Higher attack damage.
			///		Slow build up.
			/// -> Swing
			///		Very fast build up.
			///		Low damage
			/// </remarks>
			Uppercut = 1,

			/// <summary>
			/// Stabbing enemies via piercing
			/// </summary>
			/// <remarks>
			///	- Low damage
			/// - Normal movement speed
			/// - Does not keep velocity
			/// - ++Stab power
			/// - +Crush power
			///
			/// Combo with:
			///	-> Uppercut:
			///		The damage will be a bit lower
			///		Low build up
			///		Transfer current movement speed into momentum for Uppercut
			/// -> Stab:
			///		The next two consecutive stabs will have a very small build up.
			///		Less Slash and Crush power per consecutive stab
			/// -> Swing:
			///		Normal build up
			///		High damage.
			/// </remarks>
			/// 
			Stab = 2,

			/// <summary>
			/// Downward slice
			/// </summary>
			/// <remarks>
			///	- Slow initial movement speed
			/// - Low momentum conservation
			/// - +Slash power
			/// - +Crush power
			/// - -Stab power
			/// - Increased gravity
			///
			/// Combo with:
			/// -> Uppercut
			///		Very fast build up
			///		Low damage
			/// -> Stab
			///		Dash to the enemy
			/// -> Swing
			///		Spin your pike
			///		Decreased stab power
			///		Increased slash and crush power
			/// </remarks>
			Swing = 3,
		}

		public enum ECombo
		{
			None,
			
			Stab,
			StabStab,
			StabStabStab,
			// stab then uppercut
			Slash,
			
			Swing,
			// swing multiple time
			Spin,
			// swing then stab
			Dash,
			
			// swing - uppercut
			PingPongUp,
			PingPongDown,

			Uppercut,
			UppercutThenStab,
		}
	}

	public struct WooyariMultiAttackQueuedInputs : IComponentBuffer
	{
		public WooyariMultiAttackAbility.EAttackType Type;

		public WooyariMultiAttackQueuedInputs(WooyariMultiAttackAbility.EAttackType input)
		{
			Type = input;
		}
	}

	public class WooyariMultiAttackAbilityProvider : BaseRuntimeRhythmAbilityProvider<WooyariMultiAttackAbility>
	{
		protected override string FilePathPrefix => "pike";

		public WooyariMultiAttackAbilityProvider(WorldCollection collection) : base(collection)
		{
			DefaultConfiguration = new WooyariMultiAttackAbility
			{
				MaxAttacksPerCommand = 4,
				DelayBeforeAttack    = TimeSpan.FromSeconds(0.1f),
				PauseAfterAttack     = TimeSpan.FromSeconds(0.1f),
			};
		}

		public override string MasterServerId => resPath.Create(new[] {"ability", "pike", "multi_atk"}, ResPath.EType.MasterServer);

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<AttackCommand>();
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			base.GetComponents(entityComponents);

			entityComponents.AddRange(new[]
			{
				GameWorld.AsComponentType<WooyariMultiAttackQueuedInputs>()
			});
			
			// Needed for Hitbox
			entityComponents.AddRange(new[]
			{
				GameWorld.AsComponentType<Position>(),
				GameWorld.AsComponentType<HitBoxAgainstEnemies>(),
				GameWorld.AsComponentType<HitBoxHistory>(),
				GameWorld.AsComponentType<DamageFrameData>(),
			});
		}
	}
}
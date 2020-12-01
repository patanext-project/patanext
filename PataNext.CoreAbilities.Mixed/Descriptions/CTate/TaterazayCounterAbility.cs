using System;
using BepuPhysics.Collidables;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay.HitBoxes;
using StormiumTeam.GameBase.Physics.Systems;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Mixed.CTate
{
	public struct TaterazayCounterAbility : ISimpleAttackAbility
	{
		public int PreviousActivation;
		
		public double DamageStock;
		public float  SendBackDamageFactorOnTrigger;
		public float  SendBackDamageFactorAfterTrigger;

		public TimeSpan AttackStart       { get; set; }
		public bool     DidAttack         { get; set; }
		public TimeSpan Cooldown          { get; set; }
		public TimeSpan DelayBeforeAttack { get; set; }
		public TimeSpan PauseAfterAttack  { get; set; }
	}

	public class TaterazayCounterAbilityProvider : BaseRuntimeRhythmAbilityProvider<TaterazayCounterAbility>
	{
		private PhysicsSystem physicsSystem;

		public TaterazayCounterAbilityProvider(WorldCollection collection) : base(collection)
		{
			DefaultConfiguration = new TaterazayCounterAbility
			{
				SendBackDamageFactorOnTrigger    = 1,
				SendBackDamageFactorAfterTrigger = 0.25f,
				DelayBeforeAttack                = TimeSpan.FromSeconds(0.5f),
				PauseAfterAttack                 = TimeSpan.Zero
			};

			DependencyResolver.Add(() => ref physicsSystem);
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
				GameWorld.AsComponentType<Position>(),
				GameWorld.AsComponentType<HitBoxAgainstEnemies>(),
				GameWorld.AsComponentType<HitBoxHistory>(),
			});
		}

		public override void SetEntityData(GameEntityHandle entity, CreateAbility data)
		{
			base.SetEntityData(entity, data);

			GameWorld.GetComponentData<AbilityActivation>(entity).DefaultCooldownOnActivation = 2;

			physicsSystem.SetColliderShape(entity, new Sphere(4));
		}
	}
}
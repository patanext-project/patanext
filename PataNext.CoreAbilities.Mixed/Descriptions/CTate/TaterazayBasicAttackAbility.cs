using System;
using System.Numerics;
using System.Text.Json;
using BepuPhysics.Collidables;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using Newtonsoft.Json;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay.HitBoxes;
using StormiumTeam.GameBase.Physics.Systems;
using StormiumTeam.GameBase.Transform.Components;

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
		private PhysicsSystem physicsSystem;

		public TaterazayBasicAttackAbilityProvider(WorldCollection collection) : base(collection)
		{
			DefaultConfiguration = new TaterazayBasicAttackAbility
			{
				DelayBeforeAttack = TimeSpan.FromSeconds(0.15),
				PauseAfterAttack  = TimeSpan.FromSeconds(0.5f)
			};

			DependencyResolver.Add(() => ref physicsSystem);
		}

		protected override string FilePathPrefix => "tate";
		public override    string MasterServerId => resPath.Create(new[] {"ability", "tate", "def_atk"}, ResPath.EType.MasterServer);

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<AttackCommand>();
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

		public override void SetEntityData(GameEntity entity, CreateAbility data)
		{
			base.SetEntityData(entity, data);

			physicsSystem.SetColliderShape(entity, new Sphere(3));
		}
	}
}
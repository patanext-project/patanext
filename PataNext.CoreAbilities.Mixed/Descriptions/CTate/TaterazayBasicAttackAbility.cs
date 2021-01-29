using System;
using Box2D.NetStandard.Collision.Shapes;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.TabEcs;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Game.GamePlay.Damage;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay.HitBoxes;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Mixed.CTate
{
	public struct TaterazayBasicAttackAbility : SimpleAttackAbility.ISettings
	{
		public TimeSpan DelayBeforeAttack { get; set; }
		public TimeSpan PauseAfterAttack  { get; set; }

		public struct Snapshot : IReadWriteSnapshotData<Snapshot>, ISnapshotSyncWithComponent<TaterazayBasicAttackAbility>
		{
			public uint Tick { get; set; }

			public long DelayBeforeAttack, PauseAfterAttack;

			public void Serialize(in BitBuffer buffer, in Snapshot baseline, in EmptySnapshotSetup setup)
			{
				buffer.AddLongDelta(DelayBeforeAttack, baseline.DelayBeforeAttack);
				buffer.AddLongDelta(PauseAfterAttack, baseline.PauseAfterAttack);
			}

			public void Deserialize(in BitBuffer buffer, in Snapshot baseline, in EmptySnapshotSetup setup)
			{
				DelayBeforeAttack = buffer.ReadLongDelta(baseline.DelayBeforeAttack);
				PauseAfterAttack  = buffer.ReadLongDelta(baseline.PauseAfterAttack);
			}

			public void FromComponent(in TaterazayBasicAttackAbility component, in EmptySnapshotSetup setup)
			{
				DelayBeforeAttack = component.DelayBeforeAttack.Ticks;
				PauseAfterAttack  = component.PauseAfterAttack.Ticks;
			}

			public void ToComponent(ref TaterazayBasicAttackAbility component, in EmptySnapshotSetup setup)
			{
				component.DelayBeforeAttack = TimeSpan.FromTicks(DelayBeforeAttack);
				component.PauseAfterAttack  = TimeSpan.FromTicks(PauseAfterAttack);
			}
		}

		public class Serializer : DeltaSnapshotSerializerBase<Snapshot, TaterazayBasicAttackAbility>
		{
			public Serializer(ISnapshotInstigator instigator, Context ctx) : base(instigator, ctx)
			{
				CheckDifferenceSettings = true;
			}
		}

		public struct State : SimpleAttackAbility.IState
		{
			public TimeSpan AttackStart { get; set; }
			public bool     DidAttack   { get; set; }
			public TimeSpan Cooldown    { get; set; }

			public struct Snapshot : IReadWriteSnapshotData<Snapshot>, ISnapshotSyncWithComponent<State>
			{
				public uint Tick { get; set; }

				public long AttackStart, Cooldown;

				public void Serialize(in BitBuffer buffer, in Snapshot baseline, in EmptySnapshotSetup setup)
				{
					buffer.AddLongDelta(AttackStart, baseline.AttackStart);
					buffer.AddLongDelta(Cooldown, baseline.Cooldown);
				}

				public void Deserialize(in BitBuffer buffer, in Snapshot baseline, in EmptySnapshotSetup setup)
				{
					AttackStart = buffer.ReadLongDelta(baseline.AttackStart);
					Cooldown    = buffer.ReadLongDelta(baseline.Cooldown);
				}

				public void FromComponent(in State component, in EmptySnapshotSetup setup)
				{
					AttackStart = component.AttackStart.Ticks;
					Cooldown    = component.Cooldown.Ticks;
				}

				public void ToComponent(ref State component, in EmptySnapshotSetup setup)
				{
					component.AttackStart = TimeSpan.FromTicks(AttackStart);
					component.Cooldown    = TimeSpan.FromTicks(Cooldown);
				}
			}

			public class Serializer : DeltaSnapshotSerializerBase<Snapshot, State>
			{
				public Serializer(ISnapshotInstigator instigator, Context ctx) : base(instigator, ctx)
				{
					CheckDifferenceSettings = true;
				}

				protected override IAuthorityArchetype? GetAuthorityArchetype()
				{
					return AuthoritySerializer<SimulationAuthority>.CreateAuthorityArchetype(GameWorld);
				}
			}
		}
	}

	public class TaterazayBasicAttackAbilityProvider : BaseRuntimeRhythmAbilityProvider<TaterazayBasicAttackAbility>
	{
		public TaterazayBasicAttackAbilityProvider(WorldCollection collection) : base(collection)
		{
			DefaultConfiguration = new TaterazayBasicAttackAbility
			{
				DelayBeforeAttack = TimeSpan.FromSeconds(0.2),
				PauseAfterAttack  = TimeSpan.FromSeconds(0.35)
			};
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
				GameWorld.AsComponentType<TaterazayBasicAttackAbility.State>(),
				GameWorld.AsComponentType<Position>(),
				GameWorld.AsComponentType<DamageFrameData>(),
				GameWorld.AsComponentType<HitBoxAgainstEnemies>(),
				GameWorld.AsComponentType<HitBoxHistory>(),
			});
		}

		public override void SetEntityData(GameEntityHandle entity, CreateAbility data)
		{
			base.SetEntityData(entity, data);

			SetDefaultEntityDataOnNonAssigned(entity);
		}
	}
}
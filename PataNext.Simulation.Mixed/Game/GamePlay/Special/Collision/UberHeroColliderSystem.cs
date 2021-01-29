using System;
using System.Collections.Generic;
using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Special;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Physics.Systems;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.Game.GamePlay.Special.Collision
{
	[UpdateBefore(typeof(UpdateDriverSystem), typeof(SendSnapshotSystem))]
	public class UberHeroColliderSystem : GameAppSystem, IPostUpdateSimulationPass
	{
		public const float HeroModeScaling = 1.325f;
		
		private Box2DPhysicsSystem physicsSystem;
		private Shape              bodyShapeSettings;

		private PooledList<Shape> pooledShapes;

		private Shape[] bodyShapes;
		private Shape[] greatShieldShapes;

		public UberHeroColliderSystem([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref physicsSystem);

			bodyShapeSettings    = new PolygonShape(0.5f, 0.75f, new Vector2(0, 0.75f), 0);
			pooledShapes = new PooledList<Shape>(ClearMode.Always);

			bodyShapes        = Array.Empty<Shape>();
			greatShieldShapes = Array.Empty<Shape>();
		}

		private EntityQuery withoutColliderQuery;
		private EntityQuery query;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			var @base = CreateEntityQuery(new[] {typeof(Position), typeof(UberHeroCollider)});
			withoutColliderQuery = QueryWithout(@base, new[] {typeof(PhysicsCollider)});
			query                = QueryWith(@base, new[] {typeof(PhysicsCollider)});
		}

		public void OnAfterSimulationUpdate()
		{
			foreach (ref var entity in withoutColliderQuery)
			{
				physicsSystem.AssignCollider(entity, bodyShapeSettings);
				entity = default; // swapback
			}

			foreach (var entity in query)
			{
				pooledShapes.Clear();

				TryGetComponentData(entity, out var direction, UnitDirection.Right);

				{
					ref var bodyShape = ref GameWorld.Boards.Entity.GetColumn(entity.Id, ref bodyShapes);
					bodyShape ??= bodyShapeSettings.Clone();

					var halfWidth  = 0.5f;
					var halfHeight = 0.75f;
					
					if (TryGetComponentData(entity, out OwnerActiveAbility ownerActiveAbility)
					    && GetComponentDataOrDefault<AbilityActivation>(ownerActiveAbility.Active).Type.HasFlag(EAbilityActivationType.HeroMode))
					{
						halfWidth *= HeroModeScaling;
						halfHeight *= HeroModeScaling;
					}
					
					((PolygonShape) bodyShape).SetAsBox(halfWidth, halfHeight, new Vector2(0, halfHeight), 0);
				}
				
				pooledShapes.Add(bodyShapeSettings);
				if (TryGetComponentData(entity, out GreatShield greatShield)
				    && TryGetComponentData(entity, out UnitPlayState playState))
				{
					ref var gsShape = ref GameWorld.Boards.Entity.GetColumn(entity.Id, ref greatShieldShapes);
					gsShape ??= new EdgeShape(new Vector2(0, 0), new Vector2(0, 1));

					if (greatShield.ForceVertexPosition == false)
					{
						if (greatShield.ForceScale == false)
							greatShield.Scale = 1 - playState.ReceiveDamagePercentage;
						
						greatShield.VertBottom = direction.UnitX * new Vector2(1.35f + greatShield.Scale * 0.5f, 0);

						greatShield.VertTop = direction.UnitX + new Vector2(-direction.Value * (1.38f + greatShield.Scale * 0.5f), 2.5f + greatShield.Scale * 1.8f);

						if (TryGetComponentData(entity, out OwnerActiveAbility ownerActiveAbility)
						    && TryGetComponentData(ownerActiveAbility.Active, out AbilityActivation activation)
						    && (activation.Type & EAbilityActivationType.HeroMode) != 0)
						{
							greatShield.VertBottom *= 1.325f;
							greatShield.VertTop    *= 1.325f;
						}
					}

					((EdgeShape) gsShape).SetTwoSided(greatShield.VertBottom, greatShield.VertTop);

					pooledShapes.Add(gsShape);
				}

				physicsSystem.AssignCollider(entity, pooledShapes.Span);
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Numerics;
using BepuUtilities;
using Collections.Pooled;
using GameHost.Simulation.TabEcs;
using PataNext.Module.Simulation.Components.Army;
using PataNext.Module.Simulation.Components.GamePlay.Special;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Hideout;
using PataNext.Module.Simulation.Game.Providers;
using PataNext.Module.Simulation.Game.Visuals;
using PataNext.Module.Simulation.GameModes.DataCoopMission;
using PataNext.Module.Simulation.Network.Snapshots;
using StormiumTeam.GameBase.Camera.Components;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.GameModes
{
	public partial class CoopMissionSystem
	{
		private void SpawnArmy()
		{
			SpawnLocalArmy();
		}

		private void SpawnLocalArmy()
		{
			var squads   = new PooledList<GameEntity>();
			var uberHero = default(GameEntity);

			// Create Units
			using var formationQuery = CreateEntityQuery(new[] {typeof(ArmyFormationDescription)});
			foreach (var formationHandle in formationQuery)
			{
				var squadBuffer = GetBuffer<OwnedRelative<ArmySquadDescription>>(formationHandle);
				foreach (var ownedSquad in squadBuffer)
				{
					var offset = TryGetComponentData(ownedSquad.Target, out InGameSquadIndexFromCenter squadIndexFromCenter)
						? squadIndexFromCenter.Value * 1.5f
						: 0.0f;

					SafeEntityFocus runtimeSquad;
					squads.Add((runtimeSquad = Focus(Safe(squadProvider.SpawnEntityWithArguments(new()
					{
						Offset = offset
					})))).Entity);

					var unitBuffer = GetBuffer<OwnedRelative<ArmyUnitDescription>>(ownedSquad.Target);
					for (var unitIdx = 0; unitIdx < unitBuffer.Count; unitIdx++)
					{
						var ownedUnit    = unitBuffer[unitIdx];
						var player       = GetComponentData<Relative<PlayerDescription>>(ownedUnit.Target).Target;
						var target       = GetComponentData<Relative<UnitTargetDescription>>(player).Target;
						var rhythmEngine = GetComponentData<Relative<RhythmEngineDescription>>(player).Target;

						var runtimeUnit = unitProvider.SpawnEntityWithArguments(new()
						{
							Base = new()
							{
								Direction = UnitDirection.Right,
								Statistics = GetComponentData<UnitStatistics>(ownedUnit.Target)
							},
							Team         = playerTeam,
							Player       = player,
							UnitTarget   = target,
							RhythmEngine = rhythmEngine
						});

						var ownedUnitFocus   = Focus(ownedUnit.Target);
						var runtimeUnitFocus = Focus(Safe(runtimeUnit));

						runtimeUnitFocus.GetData<UnitBodyCollider>().Scale = 1f;

						runtimeUnitFocus.AddData<SimulationAuthority>();
						runtimeUnitFocus.AddData<MovableAreaAuthority>();

						if (ownedUnitFocus.Has<UnitTargetControlTag>())
						{
							runtimeUnitFocus.AddData<UnitTargetControlTag>();

							AddComponent(player, new ServerCameraState
							{
								Data =
								{
									Mode   = CameraMode.Forced,
									Offset = RigidTransform.Identity,
									Target = runtimeUnitFocus.Entity
								}
							});
						}

						if (ownedUnitFocus.Has<UnitDisplayedEquipment>())
							GameWorld.Copy(ownedUnit.Target.Handle, runtimeUnit, AsComponentType<UnitDisplayedEquipment>());

						if (TryGetComponentData(ownedUnit.Target, out UnitArchetype archetype))
							AddComponent(runtimeUnit, archetype);
						if (TryGetComponentData(ownedUnit.Target, out UnitCurrentKit kit))
							AddComponent(runtimeUnit, kit);
						if (TryGetComponentData(ownedUnit.Target, out EntityVisual entityVisual))
							AddComponent(runtimeUnit, entityVisual);

						if (!HasComponent<InGameSquadIndexFromCenter>(ownedSquad.Target))
						{
							var kitResource = ownedUnitFocus.GetData<UnitCurrentKit>().Resource;
							if (TryGetComponentData(kitResource.Entity, out UnitSquadArmySelectorFromCenter squadSelector))
							{
								var o = (squadSelector.Value * 1.5f) + runtimeUnitFocus.GetData<UnitDirection>().Value * 0.5f;
								runtimeUnitFocus.AddData(new UnitTargetOffset
								{
									Idle   = o,
									Attack = o
								});
							}
							else if (TryGetComponentData(kitResource.Entity, out UnitTargetOffset kitOffset))
							{
								kitOffset.Attack = kitOffset.Idle;
								runtimeUnitFocus.AddData(kitOffset);
							}
						}
						else
						{
							runtimeUnitFocus.AddData(new UnitTargetOffset
							{
								Idle   = offset + UnitTargetOffset.CenterComputeV1(unitIdx, unitBuffer.Count, 0.5f),
								Attack = UnitTargetOffset.CenterComputeV1(unitIdx, unitBuffer.Count, 0.5f),
							});
						}

						if (TryGetComponentBuffer<UnitDefinedAbilities>(ownedUnit.Target, out var definedAbilityBuffer))
						{
							foreach (var definedAbility in definedAbilityBuffer)
							{
								var runtimeAbility = abilityCollection.SpawnFor(definedAbility.Id.ToString(), runtimeUnit, definedAbility.Selection);
								AddComponent(runtimeAbility, new SimulationAuthority());
							}
						}

						runtimeUnitFocus.GetData<Position>().Value = GetComponentData<Position>(target).Value
						                                             + (Vector3.UnitX * runtimeUnitFocus.GetData<UnitTargetOffset>().Idle);

						runtimeSquad.GetBuffer<SquadEntityContainer>()
						            .AddReinterpret(runtimeUnitFocus.Entity);
					}
				}
			}
		}
	}
}
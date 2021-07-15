using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Native.Char;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using PataNext.MasterServerShared.Services;
using PataNext.Module.Simulation.Components.Army;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Hideout;
using PataNext.Module.Simulation.Components.Network;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Network.MasterServer.Services;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.MasterServer.Utility;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using ZLogger;

namespace PataNext.Module.Simulation.Game.Hideout
{
	public class SetLocalArmyFormationSystem : GameAppSystem
	{
		private ILogger logger;
		private UnitStatusEffectComponentProvider statusEffectProvider;

		public SetLocalArmyFormationSystem([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref logger);
			DependencyResolver.Add(() => ref statusEffectProvider);
		}

		private EntityQuery localPlayerQuery;
		private EntityQuery uninitializedArmyQuery;
		private EntityQuery armyQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			localPlayerQuery = CreateEntityQuery(new[]
			{
				typeof(PlayerDescription),
				typeof(PlayerAttachedGameSave),
				typeof(PlayerIsLocal)
			});

			uninitializedArmyQuery = CreateEntityQuery(new[]
			{
				AsComponentType<LocalArmyFormation>()
			}, new[]
			{
				AsComponentType<LocalArmyContinuousRequest>(),
				AsComponentType<ArmyFormationDescription>()
			});

			armyQuery = CreateEntityQuery(new[]
			{
				AsComponentType<LocalArmyFormation>(),
				AsComponentType<LocalArmyContinuousRequest>(),
				AsComponentType<ArmyFormationDescription>()
			});
		}

		private IScheduler initScheduler = new Scheduler();

		public override bool CanUpdate()
		{
			return localPlayerQuery.Any() && base.CanUpdate();
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var player = localPlayerQuery.GetEnumerator().First;

			// We check if the query isn't empty, so that we don't allocate at each frame in the foreach loop
			if (uninitializedArmyQuery.Any())
			{
				uninitializedArmyQuery.ForEachDeferred((ent, player) =>
				{
					var saveGuid = GetComponentData<PlayerAttachedGameSave>(player).Guid;

					var request = World.Mgr.CreateEntity();
					request.Set(new GetFormationRequest {SaveId = saveGuid.ToString()});

					AddComponent(ent, new LocalArmyContinuousRequest {Source = request});
					AddComponent(ent, new ArmyFormationDescription());
					AddComponent(ent, new Relative<PlayerDescription>(Safe(player)));
					AddComponent(ent, new Owner(Safe(player)));
					GameWorld.AddBuffer<OwnedRelative<ArmySquadDescription>>(ent);

					GameWorld.AssureComponents(player, new[]
					{
						AsComponentType<OwnedRelative<ArmySquadDescription>>(),
						AsComponentType<OwnedRelative<ArmyUnitDescription>>()
					});
				}, player, initScheduler);
			}

			initScheduler.Run();

			foreach (var formationHandle in armyQuery)
			{
				ref readonly var continuousRequest = ref GetComponentData<LocalArmyContinuousRequest>(formationHandle);
				if (continuousRequest.Source == default)
				{
					logger.ZLogWarning($"Invalid continuous request on {formationHandle}");
					continue;
				}

				if (continuousRequest.Source.TryGet(out GetFormationRequest.Response response))
				{
					logger.ZLogInformation("received response!");

					const int additionalSquad = 2; // UberHero and Hatapon squad

					var resultSquads     = response.Result.Squads;
					var ownedSquadBuffer = GetBuffer<OwnedRelative<ArmySquadDescription>>(formationHandle);

					var squadCount = response.Result.Squads.Length + additionalSquad;
					for (var squadIdx = 0; squadIdx < Math.Max(ownedSquadBuffer.Count, squadCount); squadIdx++)
					{
						#region Squad creation/destruction

						if (ownedSquadBuffer.Count <= squadIdx)
						{
							// Create squad
							var squadEntity = CreateEntity();
							AddComponent(Safe(squadEntity),
								new ArmySquadDescription(),
								new Owner(Safe(formationHandle)),
								new Relative<ArmyFormationDescription>(Safe(formationHandle)),
								GetComponentData<Relative<PlayerDescription>>(formationHandle),

								new LocalSquadType((ELocalSquadType) squadIdx)
							);

							GameWorld.AddComponent(squadEntity, AsComponentType<OwnedRelative<ArmyUnitDescription>>());
							GameWorld.AddComponent(squadEntity, new HideoutSquadIndex(squadIdx - 1));

							if ((ELocalSquadType) squadIdx != ELocalSquadType.UberHero)
							{
								GameWorld.AddComponent(squadEntity, new InGameSquadIndexFromCenter((ELocalSquadType) squadIdx switch
								{
									ELocalSquadType.SquadTate => 2,
									ELocalSquadType.SquadYari => 1,
									ELocalSquadType.SquadYumi => -1,
									ELocalSquadType.Hatapon => 0,
									_ => throw new ArgumentOutOfRangeException(nameof(squadIdx))
								}));
							}

							ownedSquadBuffer.Add(new(Safe(squadEntity)));
						}
						else if (ownedSquadBuffer.Count > squadIdx && ownedSquadBuffer.Count > squadCount && squadIdx >= squadCount)
						{
							// Destroy squad
							RemoveEntity(ownedSquadBuffer[squadIdx].Target);
							ownedSquadBuffer.RemoveAt(squadIdx--);

							Console.WriteLine($"Destroyed Squad {squadIdx}");
							continue;
						}

						var squadFocus = Focus(ownedSquadBuffer[squadIdx].Target);
						squadFocus.ThrowIfNotExists();

						var squadData = squadIdx > 1
							? resultSquads[squadIdx - 2]
							: new()
							{
								Leader = (ELocalSquadType) squadIdx switch
								{
									ELocalSquadType.Hatapon => response.Result.FlagBearer,
									ELocalSquadType.UberHero => response.Result.UberHero,
									_ => throw new("nothing found for " + (ELocalSquadType) squadIdx)
								},
								Soldiers = Array.Empty<string>()
							};

						#endregion

						#region Unit Management in Squad

						var unitBuffer = squadFocus.GetBuffer<OwnedRelative<ArmyUnitDescription>>()
						                           .Reinterpret<GameEntity>();


						// - UberHero and Hatapon squad = 1 unit max
						// - Other squads are based on their length + Leader
						var unitCount = squadData.Soldiers.Length + 1;
						for (var unitIdx = 0; unitIdx < unitCount; unitIdx++)
						{
							if (unitBuffer.Count <= unitIdx)
							{
								// Create unit
								var unitEntity = CreateEntity();
								AddComponent(Safe(unitEntity),
									new Relative<ArmySquadDescription>(squadFocus.Entity),
									new Relative<ArmyFormationDescription>(Safe(formationHandle)),
									new Relative<PlayerDescription>(Safe(player)),
									new ArmyUnitDescription(),
									new Owner(squadFocus.Entity)
								);

								if ((ELocalSquadType)squadIdx == ELocalSquadType.Hatapon)
									AddComponent(unitEntity, new UnitTargetControlTag());

								GameWorld.AddComponent(unitEntity, AsComponentType<UnitDefinedAbilities>());
								GameWorld.AddComponent(unitEntity, AsComponentType<UnitDisplayedEquipment>());
								GameWorld.AddComponent(unitEntity, AsComponentType<UnitDefinedEquipments>());
								GameWorld.AddComponent(unitEntity, AsComponentType<UnitAllowedEquipment>());

								GameWorld.AddComponent(unitEntity, AsComponentType<UnitArchetype>());
								GameWorld.AddComponent(unitEntity, AsComponentType<UnitCurrentKit>());
								GameWorld.AddComponent(unitEntity, AsComponentType<UnitStatistics>());
								GameWorld.AddComponent(unitEntity, AsComponentType<UnitIndexInSquad>());

								foreach (var statusType in new[]
								{
									typeof(PataNext.Game.Abilities.Effects.Critical),
									typeof(PataNext.Game.Abilities.Effects.KnockBack),
									typeof(PataNext.Game.Abilities.Effects.Stagger),
									typeof(PataNext.Game.Abilities.Effects.Burn),
									typeof(PataNext.Game.Abilities.Effects.Sleep),
									typeof(PataNext.Game.Abilities.Effects.Freeze),
									typeof(PataNext.Game.Abilities.Effects.Poison),
									typeof(PataNext.Game.Abilities.Effects.Tumble),
									typeof(PataNext.Game.Abilities.Effects.Wind),
									typeof(PataNext.Game.Abilities.Effects.Piercing),
									typeof(PataNext.Game.Abilities.Effects.Silence),
								})
								{
									statusEffectProvider.AddStatus(unitEntity, GameWorld.AsComponentType(statusType), new());
								}
								

								unitBuffer.Add(Safe(unitEntity));

								//Console.WriteLine($"Created Unit {unitIdx} of squad {(ELocalSquadType)squadIdx} {unitEntity} (MID {squadData.Leader})");
							}
							else if (unitBuffer.Count > unitIdx && unitBuffer.Count > unitCount && unitIdx >= unitCount)
							{
								// Destroy unit
								RemoveEntity(unitBuffer[squadIdx]);
								unitBuffer.RemoveAt(squadIdx--);

								//Console.WriteLine($"Destroyed Unit {unitIdx}");
								continue;
							}

							var unitFocus = Focus(unitBuffer[unitIdx]);
							unitFocus.AddData(new MasterServerControlledUnitData(unitIdx == 0 ? squadData.Leader : squadData.Soldiers[unitIdx - 1]))
							         // Make sure that UberHero and Hatapon is the first in their squad.
							         // The Uberhero will share the same squad that correspond to the other 
							         .AddData(new UnitIndexInSquad((ELocalSquadType)squadIdx < ELocalSquadType.SquadTate ? 0 : unitIdx));
							// ^ originally it was 'unitIdx + 1' but since I've learnt that the Uberhero is at the same index as the leader
							// it was modified, ingame the uberhero will be put in front via abilities 

							if (unitIdx == 0 && (ELocalSquadType)squadIdx >= ELocalSquadType.SquadTate)
							{
								GameWorld.UpdateOwnedComponent(squadFocus.Handle, new HideoutLeaderSquad { Leader = unitFocus.Entity });
							}
						}

						#endregion
					}

					continuousRequest.Source.Disable<GetFormationRequest>();
					continuousRequest.Source.Remove<GetFormationRequest.Response>();
				}
			}
		}
	}

	#region Components

	public struct LocalArmyFormation : IComponentData
	{

	}

	public struct LocalArmyContinuousRequest : IComponentData
	{
		public Entity Source;
	}

	public enum ELocalSquadType
	{
		Hatapon   = 0,
		UberHero  = 1,
		SquadTate = 2,
		SquadYari = 3,
		SquadYumi = 4,
	}

	public struct LocalSquadType : IComponentData
	{
		public ELocalSquadType Value;

		public bool IsHatapon     => Value == ELocalSquadType.Hatapon;
		public bool IsUberHero    => Value == ELocalSquadType.UberHero;
		public bool IsLeaderSquad => Value >= ELocalSquadType.SquadTate && Value <= ELocalSquadType.SquadYumi;

		public LocalSquadType(ELocalSquadType value) => Value = value;
	}

	#endregion
}
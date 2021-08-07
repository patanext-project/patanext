using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Revolution.NetCode.LLAPI;
using GameHost.Simulation.Application;
using GameHost.Simulation.Features.ShareWorldState;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using GameHost.Threading;
using GameHost.Utility;
using GameHost.Worlds;
using MagicOnion;
using Microsoft.Extensions.Logging;
using PataNext.MasterServerShared.Services;
using PataNext.Module.Simulation;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.Special;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Game.GamePlay.FreeRoam;
using PataNext.Module.Simulation.Game.Providers;
using PataNext.Module.Simulation.GameModes.InBasement;
using PataNext.Module.Simulation.Network.MasterServer;
using PataNext.Module.Simulation.Network.NetCodeRpc;
using PataNext.Module.Simulation.Network.Snapshots;
using PataNext.Module.Simulation.Network.Snapshots.Abilities;
using PataNext.Module.Simulation.Network.Snapshots.Collision;
using PataNext.Module.Simulation.Network.Snapshots.Resources;
using PataNext.Module.Simulation.Network.Snapshots.Team;
using PataNext.Module.Simulation.Passes;
using PataNext.Module.Simulation.Resources;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Network.MasterServer.Utility;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Interfaces;

[assembly: RegisterAvailableModule("PataNext Simulation", "guerro", typeof(CustomModule))]

namespace PataNext.Module.Simulation
{
	public class CustomModule : GameHostModule
	{
		private void InjectProviders(SimulationApplication app)
		{
			app.Data.Collection.GetOrCreate(typeof(Systems.UnitStatusEffectComponentProvider));
			app.Data.Collection.GetOrCreate(typeof(Game.Providers.PlayableUnitProvider));
			app.Data.Collection.GetOrCreate(typeof(Game.Providers.PlayerTeamProvider));
			app.Data.Collection.GetOrCreate(typeof(Game.Providers.RhythmEngineProvider));
			app.Data.Collection.GetOrCreate(typeof(Game.Providers.TeamEndFlagProvider));
			app.Data.Collection.GetOrCreate(typeof(Game.Providers.SimpleDestroyableStructureProvider));
			app.Data.Collection.GetOrCreate(typeof(GameModes.DataCoopMission.CoopMissionPlayerTeamProvider));
			app.Data.Collection.GetOrCreate(typeof(GameModes.DataCoopMission.CoopMissionPlayableUnitProvider));
			app.Data.Collection.GetOrCreate(typeof(GameModes.DataCoopMission.CoopMissionUnitTargetProvider));
			app.Data.Collection.GetOrCreate(typeof(GameModes.DataCoopMission.CoopMissionRhythmEngineProvider));
			app.Data.Collection.GetOrCreate(typeof(GameModes.DataCoopMission.CoopMissionSquadProvider));

			app.Data.Collection.GetOrCreate(typeof(Game.Providers.BastionDynamicGroupProvider));
			app.Data.Collection.GetOrCreate(typeof(Game.Providers.BastionFixedGroupProvider));
		}

		private void InjectMasterServerHubs(ApplicationData app)
		{
			void inject<THub, TReceiverInterface, TReceiverObj>()
				where THub : IStreamingHub<THub, TReceiverInterface>
				where TReceiverObj : TReceiverInterface, new()
			{
				app.Collection.GetOrCreate(typeof(HubClientConnectionCache<THub, TReceiverInterface>));
				app.Context.BindExisting<TReceiverInterface>(new TReceiverObj());
			}

			inject<IFormationHub, IFormationReceiver, HubFormationReceiver>();
			inject<IUnitHub, IUnitHubReceiver, UnitHubReceiver>();
			inject<IUnitPresetHub, IUnitPresetHubReceiver, UnitPresetHubReceiver>();
			inject<IItemHub, IItemHubReceiver, ItemHubReceiver>();
		}

		private void InjectMasterServerProcessSystems(ApplicationData app)
		{
			void inject<TSystem>() where TSystem : IWorldSystem
			{
				app.Collection.GetOrCreate(typeof(TSystem));
			}

			inject<Network.MasterServer.Services.CreateGameSaveRequest.Process>();
			inject<Network.MasterServer.Services.ListGameSaveRequest.Process>();
			inject<Network.MasterServer.Services.GetFavoriteGameSaveRequest.Process>();
			inject<Network.MasterServer.Services.SetFavoriteGameSaveRequest.Process>();

			inject<Network.MasterServer.Services.GetFormationRequest.Process>();

			inject<Network.MasterServer.Services.GetUnitDetailsRequest.Process>();

			inject<Network.MasterServer.Services.GetSaveUnitPresetsRequest.Process>();
			inject<Network.MasterServer.Services.GetUnitPresetDetailsRequest.Process>();
			inject<Network.MasterServer.Services.GetUnitPresetEquipmentsRequest.Process>();
			inject<Network.MasterServer.Services.GetUnitPresetAbilitiesRequest.Process>();
			inject<Network.MasterServer.Services.CopyPresetToTargetUnitRequest.Process>();
			inject<Network.MasterServer.Services.SetPresetEquipments.Process>();

			inject<Network.MasterServer.Services.GetItemAssetPointerRequest.Process>();
			inject<Network.MasterServer.Services.GetItemDetailsRequest.Process>();
			inject<Network.MasterServer.Services.GetInventoryRequest.Process>();

			// Full Fledged systems
			inject<Network.MasterServer.Services.FullFledged.GetAndSetFullUnitsPresetDetailsRequest.Process>();
		}

		private void InjectFreeRoamSystems(SimulationApplication app)
		{
			app.Data.Collection.GetOrCreate(typeof(FreeRoamUnitProvider));
			app.Data.Collection.GetOrCreate(typeof(FreeRoamCharacterMovementSystem));
			app.Data.Collection.GetOrCreate(typeof(CharacterEnterCityLocationSystem));
			app.Data.Collection.GetOrCreate(typeof(SynchronizeCharacterVisualSystem));
		}

		private void InjectSerializers(SimulationApplication app, SerializerCollection sc)
		{
			var appCtx = app.Data.Context;

			RegisterSnapshots();
			RegisterDescription();
			RegisterAuthority();

			void RegisterSnapshots()
			{
				Rpc();

				Resources();
				Input();
				GameMode();
				GamePlay();

				void Rpc()
				{
					sc.Register(instigator => new DamageRequestRpc.Serializer(appCtx));
				}

				void Resources()
				{
					sc.Register(instigator => new GameGraphicResourceSnapshot.Serializer(instigator, appCtx));
					sc.Register(instigator => new EquipmentResourceSnapshot.Serializer(instigator, appCtx));

					sc.Register(instigator => new UnitArchetypeResourceSnapshot.Serializer(instigator, appCtx));
					sc.Register(instigator => new UnitKitResourceSnapshot.Serializer(instigator, appCtx));
					sc.Register(instigator => new UnitAttachmentResourceSnapshot.Serializer(instigator, appCtx));

					sc.Register(instigator => new RhythmCommandResourceSnapshot.Serializer(instigator, appCtx));
					sc.Register(instigator => new RhythmCommandIdentifierSnapshot.Serializer(instigator, appCtx));
				}

				void Input()
				{
					sc.Register(instigator => new GameRhythmInputSnapshot.Serializer(instigator, appCtx));
					sc.Register(instigator => new FreeRoamInputSnapshot.Serializer(instigator, appCtx));
				}

				void GameMode()
				{

				}

				void GamePlay()
				{
					Ability();
					Unit();
					Collision();
					RhythmEngine();
					Team();

					void Ability()
					{
						sc.Register(instigator => new OwnerActiveAbilitySnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new AbilityStateSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new AbilityEngineSetSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new AbilityControlVelocitySnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new AbilityActivationSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new AbilityCommandsSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new AbilityModifyStatsOnChainingSnapshot.Serializer(instigator, appCtx));
					}

					void Unit()
					{
						sc.Register(instigator => new UnitStatisticSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new UnitPlayStateSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new UnitEnemySeekingStateSnapshot.Serializer(instigator, appCtx));

						sc.Register(instigator => new UnitArchetypeSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new UnitCurrentKitSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new UnitDisplayedEquipmentSnapshot.Serializer(instigator, appCtx));

						sc.Register(instigator => new UnitDirectionSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new UnitTargetOffsetSnapshot.Serializer(instigator, appCtx));

						sc.Register(instigator => new UnitControllerStateSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new UnitFreeRoamMovementSnapshot.Serializer(instigator, appCtx));

						sc.Register(instigator => new GroundStateSnapshot.Serializer(instigator, appCtx));
					}

					void Collision()
					{
						sc.Register(instigator => new UberHeroColliderSnapshot.Serializer(instigator, appCtx));
					}

					void RhythmEngine()
					{
						sc.Register(instigator => new GameComboStateSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new GameComboSettingsSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new GameCommandStateSnapshot.Serializer(instigator, appCtx));

						sc.Register(instigator => new RhythmEngineCommandProgressBufferSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new RhythmEnginePredictedCommandBufferSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new RhythmCommandActionBufferSnapshot.Serializer(instigator, appCtx));

						sc.Register(instigator => new RhythmEngineControllerSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new RhythmEngineExecutingCommandSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new RhythmEngineLocalStateSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new RhythmEngineSettingsSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new RhythmSummonEnergySnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new RhythmSummonEnergyMaxSnapshot.Serializer(instigator, appCtx));

						// Commands
						sc.Register(instigator => new ChargeCommand.Serializer(instigator, appCtx));
					}

					void Team()
					{
						sc.Register(instigator => new TeamMovableAreaSnapshot.Serializer(instigator, appCtx));
						sc.Register(instigator => new ContributeToTeamMovableAreaSnapshot.Serializer(instigator, appCtx));
					}
				}
			}

			void RegisterDescription()
			{
				sc.Register(instigator => new IEntityDescription.Serializer<RhythmEngineDescription>(instigator, appCtx));
				sc.Register(instigator => new IEntityDescription.Serializer<UnitTargetDescription>(instigator, appCtx));
				sc.Register(instigator => new IEntityDescription.Serializer<UnitDescription>(instigator, appCtx));
				sc.Register(instigator => new IEntityDescription.Serializer<MountDescription>(instigator, appCtx));
				sc.Register(instigator => new IEntityDescription.Serializer<AbilityDescription>(instigator, appCtx));
				sc.Register(instigator => new IEntityDescription.Serializer<ProjectileDescription>(instigator, appCtx));

				sc.Register(instigator => new Relative<RhythmEngineDescription>.Serializer(instigator, appCtx));
				sc.Register(instigator => new Relative<UnitTargetDescription>.Serializer(instigator, appCtx));
				sc.Register(instigator => new Relative<UnitDescription>.Serializer(instigator, appCtx));
				sc.Register(instigator => new Relative<MountDescription>.Serializer(instigator, appCtx));
				sc.Register(instigator => new Relative<AbilityDescription>.Serializer(instigator, appCtx));
				sc.Register(instigator => new Relative<ProjectileDescription>.Serializer(instigator, appCtx));

				sc.Register(instigator => new OwnedRelative<AbilityDescription>.Serializer(instigator, appCtx));
			}

			void RegisterAuthority()
			{
				sc.Register(instigator => new AuthoritySerializer<MovableAreaAuthority>(instigator, appCtx));
			}
		}

		public CustomModule(Entity source, Context ctxParent, GameHostModuleDescription original) : base(source, ctxParent, original)
		{
			AddDisposable(ApplicationTracker.Track(this, (SimulationApplication simulationApplication) =>
			{
				var ctx = simulationApplication.Data.Context;
				ctx.BindExisting(DefaultEntity<GameResourceDb<EquipmentResource>.Defaults>.Create(simulationApplication.Data.World, new()));
				ctx.BindExisting(DefaultEntity<GameResourceDb<GameGraphicResource>.Defaults>.Create(simulationApplication.Data.World, new()));
				ctx.BindExisting(DefaultEntity<GameResourceDb<RhythmCommandResource>.Defaults>.Create(simulationApplication.Data.World, new()));
				ctx.BindExisting(DefaultEntity<GameResourceDb<UnitArchetypeResource>.Defaults>.Create(simulationApplication.Data.World, new()));
				ctx.BindExisting(DefaultEntity<GameResourceDb<UnitAttachmentResource>.Defaults>.Create(simulationApplication.Data.World, new()));
				ctx.BindExisting(DefaultEntity<GameResourceDb<UnitKitResource>.Defaults>.Create(simulationApplication.Data.World, new()));
				ctx.BindExisting(DefaultEntity<GameResourceDb<UnitRoleResource>.Defaults>.Create(simulationApplication.Data.World, new()));

				simulationApplication.Data.Collection.DefaultSystemCollection.AddPass(new IRhythmEngineSimulationPass.RegisterPass(),
					new[] { typeof(IUpdateSimulationPass.RegisterPass) },
					new[] { typeof(IPostUpdateSimulationPass.RegisterPass) });

				simulationApplication.Data.Collection.DefaultSystemCollection.AddPass(new IAbilityPreSimulationPass.RegisterPass(),
					new[] { typeof(IUpdateSimulationPass.RegisterPass), typeof(IRhythmEngineSimulationPass.RegisterPass) },
					new[] { typeof(IPostUpdateSimulationPass.RegisterPass) });

				simulationApplication.Data.Collection.DefaultSystemCollection.AddPass(new IAbilitySimulationPass.RegisterPass(),
					new[] { typeof(IUpdateSimulationPass.RegisterPass), typeof(IAbilityPreSimulationPass.RegisterPass) },
					new[] { typeof(IPostUpdateSimulationPass.RegisterPass) });

				InjectProviders(simulationApplication);
				InjectMasterServerHubs(simulationApplication.Data);
				InjectMasterServerProcessSystems(simulationApplication.Data);

				simulationApplication.Data.Collection.GetOrCreate(typeof(Network.MasterServer.Systems.SynchronizeInventorySystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.Hideout.UpdateUnitEquipmentRequestSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Network.MasterServer.Systems.MasterServerPlayerInventoryProvider));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Systems.DontSerializeAbilityEngineSet));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Systems.SpawnDefaultCommandsSystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Systems.AbilityCollectionSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Systems.KitCollectionSystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.RegisterDefaultKits));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Components.Roles.UnitDescription.RegisterContainer));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Components.Roles.AbilityDescription.RegisterContainer));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Components.Roles.MountDescription.RegisterContainer));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Components.Army.ArmyFormationDescription.RegisterContainer));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Components.Army.ArmySquadDescription.RegisterContainer));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Components.Army.ArmyUnitDescription.RegisterContainer));

				InjectFreeRoamSystems(simulationApplication);

				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Units.UnitUpdateStatusEffectSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Units.UnitCalculatePlayStateSystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Abilities.UpdateActiveAbilitySystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Abilities.ApplyAbilityStatisticOnChainingSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Abilities.ExecuteActiveAbilitySystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Team.UpdateTeamMovableAreaSystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Units.UnitPhysicsSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Abilities.AbilityControlVelocitySystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Units.UnitCalculateSeekingStateSystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Units.UnitCollisionSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Units.UnitPhysicsAfterBlockUpdateSystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Special.Collision.UberHeroColliderSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Special.Squad.UpdateSquadUnitDisplacementSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Special.Ai.SimpleAiSystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.ManageComponentTagSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.ProcessEngineSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.OnInputForRhythmEngine));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.GetNextCommandEngineSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.ApplyCommandEngineSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.RhythmEngine.Systems.RhythmEngineResizeCommandBufferSystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(GameModes.BasicTestGameModeSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(GameModes.InBasement.AtCityGameModeSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(GameModes.StartYaridaTrainingGameMode));
				simulationApplication.Data.Collection.GetOrCreate(typeof(GameModes.CoopMissionSystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Structures.EndFlagUpdateSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Structures.Bastion.BastionDynamicRecycleDeadEntitiesSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Structures.Bastion.BastionSpawnAllIfAllDeadSystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.Hideout.SetLocalArmyFormationSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.Hideout.UpdateMasterServerUnitSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.Hideout.UpdateArmyUnitStatisticsSystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Damage.GenerateDamageRequestSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Damage.ApplyDefensiveBonusesSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Damage.ApplyStatusSystem));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.GamePlay.Damage.ApplyKnockbackSystem));


				simulationApplication.Data.Collection.GetOrCreate(typeof(Game.Scenar.TestScenarProvider));

				if (simulationApplication.Data.Collection.TryGet(out SendWorldStateSystem sendWorldStateSystem))
				{
					var gameWorld = new ContextBindingStrategy(simulationApplication.Data.Context, false).Resolve<GameWorld>();
					sendWorldStateSystem.SetDisabled(gameWorld.AsComponentType<PlayerInventoryTarget>(), true);
				}

				var serializerCollection = simulationApplication.Data.Collection.GetOrCreate(wc => new SerializerCollection(wc));
				InjectSerializers(simulationApplication, serializerCollection);
			}));
		}
	}
}
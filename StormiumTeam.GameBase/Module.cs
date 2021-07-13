using DefaultEcs;
using GameHost.Core.Ecs.Passes;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Inputs.Systems;
using GameHost.Revolution.NetCode.LLAPI;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;
using StormiumTeam.GameBase.Bootstrap;
using StormiumTeam.GameBase.Camera.Components;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.GamePlay.Events;
using StormiumTeam.GameBase.GamePlay.Health.Providers;
using StormiumTeam.GameBase.GamePlay.Health.Systems;
using StormiumTeam.GameBase.GamePlay.Health.Systems.Pass;
using StormiumTeam.GameBase.GamePlay.HitBoxes;
using StormiumTeam.GameBase.Network;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Network.Components;
using StormiumTeam.GameBase.Network.MasterServer;
using StormiumTeam.GameBase.Network.MasterServer.AssetService;
using StormiumTeam.GameBase.Network.MasterServer.StandardAuthService;
using StormiumTeam.GameBase.Network.MasterServer.User;
using StormiumTeam.GameBase.Network.MasterServer.UserService;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Physics.Systems;
using StormiumTeam.GameBase.Roles;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Roles.Interfaces;
using StormiumTeam.GameBase.Time;
using StormiumTeam.GameBase.Transform.Components;

[assembly: RegisterAvailableModule("GameBase", "StormiumTeam", typeof(StormiumTeam.GameBase.Module))]

namespace StormiumTeam.GameBase
{
	public class Module : GameHostModule
	{
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
			var global = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();

			foreach (ref readonly var listener in global.World.Get<IListener>())
			{
				if (listener is SimulationApplication simulationApplication)
				{
					simulationApplication.Schedule(() =>
					{
						var systemCollection = simulationApplication.Data.Collection.DefaultSystemCollection;
						systemCollection.AddPass(new UpdateInputPassRegister(), new[] {typeof(UpdatePassRegister)}, null);
						systemCollection.AddPass(new IPreUpdateSimulationPass.RegisterPass(), new[] {typeof(UpdatePassRegister), typeof(UpdateInputPassRegister)}, null);
						systemCollection.AddPass(new IUpdateSimulationPass.RegisterPass(), new[] {typeof(IPreUpdateSimulationPass.RegisterPass)}, null);
						systemCollection.AddPass(new IPostUpdateSimulationPass.RegisterPass(), new[] {typeof(IUpdateSimulationPass.RegisterPass)}, null);
						systemCollection.AddPass(new IAfterSnapshotDataPass.RegisterPass(), null, null);
						systemCollection.AddPass(new IGameEventPass.RegisterPass(), null, null);

						systemCollection.AddPass(new RegisterHealthProcessPass(), null, null);

						//Console.WriteLine("Passes:");
						//Console.WriteLine($"{string.Join("\n  ", systemCollection.Passes.Select(p => p.ToString()))}");

						var physicsSystem = simulationApplication.Data.Collection.GetOrCreate(wc => new Box2DPhysicsSystem(wc));
						simulationApplication.Data.Context.BindExisting<IPhysicsSystem>(physicsSystem);

						{
							simulationApplication.Data.Collection.GetOrCreate(typeof(IAfterSnapshotDataPass.AddPass));
						}
						// Pre
						{
							simulationApplication.Data.Collection.GetOrCreate(typeof(SetGameTimeSystem));
							simulationApplication.Data.Collection.GetOrCreate(typeof(BuildTeamEntityContainerSystem));
							simulationApplication.Data.Collection.GetOrCreate(typeof(HealthDescription.RegisterContainer));

							simulationApplication.Data.Collection.GetOrCreate(typeof(ForceTemporaryAuthoritySystem));
						}

						// Now
						{
							simulationApplication.Data.Collection.GetOrCreate(typeof(HitBoxAgainstEnemiesSystem));
						}

						// Post
						{
							simulationApplication.Data.Collection.GetOrCreate(typeof(ModifyHealthEventProvider));
							simulationApplication.Data.Collection.GetOrCreate(typeof(HealthSystem));
							{
								simulationApplication.Data.Collection.GetOrCreate(typeof(DefaultHealthProcess));
								simulationApplication.Data.Collection.GetOrCreate(typeof(DefaultHealthProvider));
							}

							simulationApplication.Data.Collection.GetOrCreate(typeof(RemoveEntityWithEndTimeSystem));

							simulationApplication.Data.Collection.GetOrCreate(typeof(SendSnapshotSystemAtPostUpdate));
						}

						// ???
						{
							simulationApplication.Data.Collection.GetOrCreate(typeof(CreateGamePlayerOnConnectionSystem)); // perhaps in Pre?
							simulationApplication.Data.Collection.GetOrCreate(typeof(QueueNetworkedEntitySystem));         // perhaps in Pre?
							simulationApplication.Data.Collection.GetOrCreate(typeof(SetTimeOnSubOwnedSystem));            // perhaps in Pre?
							simulationApplication.Data.Collection.GetOrCreate(typeof(TimeFromForeignSystem));              // perhaps in Pre?
							simulationApplication.Data.Collection.GetOrCreate(typeof(ReportToForeignTimeSystem));          // perhaps in Pre?
							simulationApplication.Data.Collection.GetOrCreate(typeof(SetPlayerLocalSystem));               // perhaps in Pre?
							simulationApplication.Data.Collection.GetOrCreate(typeof(InstigatorTimeSystem));               // perhaps in Pre?

							simulationApplication.Data.Collection.GetOrCreate(typeof(MasterServerManageSystem));
							simulationApplication.Data.Collection.GetOrCreate(typeof(CurrentUserSystem));
							simulationApplication.Data.Collection.GetOrCreate(typeof(GetUserLoginRequest.Process));
							simulationApplication.Data.Collection.GetOrCreate(typeof(DisconnectUserRequest.Process));

							simulationApplication.Data.Collection.GetOrCreate(typeof(ConnectUserRequest.Process));
							#if !STORMIUM
							simulationApplication.Data.Collection.GetOrCreate(typeof(ConnectUserWithDiscordRequest.Process));
							#endif
							
							simulationApplication.Data.Collection.GetOrCreate(typeof(GetAssetPointerRequest.Process));
							simulationApplication.Data.Collection.GetOrCreate(typeof(GetAssetDetailsRequest.Process));
							simulationApplication.Data.Collection.GetOrCreate(typeof(GetAssetGuidRequest.Process));
						}

						var serializerCollection = simulationApplication.Data.Collection.GetOrCreate(wc => new SerializerCollection(wc));
						var ctx                  = simulationApplication.Data.Context;
						serializerCollection.Register(instigator => new IsResourceEntitySerializer(instigator, ctx));
						serializerCollection.Register(instigator => new NetForeignTimeSerializer(instigator, ctx));

						serializerCollection.Register(instigator => new AuthoritySerializer<InputAuthority>(instigator, ctx));
						serializerCollection.Register(instigator => new AuthoritySerializer<SimulationAuthority>(instigator, ctx));
						serializerCollection.Register(instigator => new Owner.Serializer(instigator, ctx));
						serializerCollection.Register(instigator => new Position.Serializer(instigator, ctx));
						serializerCollection.Register(instigator => new Velocity.Serializer(instigator, ctx));
						serializerCollection.Register(instigator => new ServerCameraState.Serializer(instigator, ctx));
						serializerCollection.Register(instigator => new AssignInstigatorTime.Serializer(instigator, ctx));
						serializerCollection.Register(instigator => new ClubInformation.Serializer(instigator, ctx));
						serializerCollection.Register(instigator => new IEntityDescription.Serializer<PlayerDescription>(instigator, ctx));
						serializerCollection.Register(instigator => new IEntityDescription.Serializer<TeamDescription>(instigator, ctx));
						serializerCollection.Register(instigator => new IEntityDescription.Serializer<ClubDescription>(instigator, ctx));
						serializerCollection.Register(instigator => new IEntityDescription.Serializer<HealthDescription>(instigator, ctx));

						serializerCollection.Register(instigator => new Relative<PlayerDescription>.Serializer(instigator, ctx));
						serializerCollection.Register(instigator => new Relative<TeamDescription>.Serializer(instigator, ctx));
						serializerCollection.Register(instigator => new Relative<ClubDescription>.Serializer(instigator, ctx));
						serializerCollection.Register(instigator => new Relative<HealthDescription>.Serializer(instigator, ctx));

						serializerCollection.Register(instigator => new OwnedRelative<HealthDescription>.Serializer(instigator, ctx));
						serializerCollection.Register(instigator => new TeamEntityContainer.Serializer(instigator, ctx));
						serializerCollection.Register(instigator => new TeamEnemies.Serializer(instigator, ctx));
						serializerCollection.Register(instigator => new TeamAllies.Serializer(instigator, ctx));

						serializerCollection.Register(instigator => new TargetDamageEvent.Serializer(instigator, ctx));

					}, default);
				}
			}
		}
	}
}
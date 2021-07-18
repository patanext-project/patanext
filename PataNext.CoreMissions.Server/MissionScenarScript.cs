using System;
using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Team;
using PataNext.Module.Simulation.Game.Providers;
using PataNext.Module.Simulation.Resources;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.Network.Authorities;

namespace PataNext.CoreMissions.Server
{
	public abstract class MissionScenarScript : ScenarScript
	{
		private PlayerTeamProvider teamProvider;

		protected AbilityCollectionSystem                abilityCollection;
		protected GameResourceDb<UnitAttachmentResource> AttachmentDb;
		protected GameResourceDb<EquipmentResource>      EquipmentDb;

		public MissionScenarScript(WorldCollection wc) : base(wc)
		{
			DependencyResolver.Add(() => ref teamProvider);
			DependencyResolver.Add(() => ref abilityCollection);
			DependencyResolver.Add(() => ref AttachmentDb);
			DependencyResolver.Add(() => ref EquipmentDb);
		}

		protected GameEntity ProtagonistTeam;
		protected GameEntity EnemyTeam;

		protected override Task OnStart()
		{
			if (!GameWorld.TryGetSingleton<ProtagonistTeamTag>(out GameEntityHandle protagonistTeamHandle))
				throw new InvalidOperationException("No protagonist team found");

			EnemyTeam = Safe(teamProvider.SpawnEntityWithArguments(new()));
			AddComponent(EnemyTeam, new SimulationAuthority());
			GetBuffer<TeamEnemies>(EnemyTeam).Add(new(Safe(protagonistTeamHandle)));
			GetBuffer<TeamEnemies>(protagonistTeamHandle).Add(new(EnemyTeam));

			GameWorld.Link(EnemyTeam.Handle, Self.Handle, true);

			ProtagonistTeam = Safe(protagonistTeamHandle);

			return Task.CompletedTask;
		}

		protected GameResource<UnitAttachmentResource> GetAttachment(string[] resource)
		{
			return AttachmentDb.GetOrCreate(ResPathGen.Create(resource, ResPath.EType.ClientResource));
		}

		protected GameResource<EquipmentResource> GetEquipment(string[] resource)
		{
			return EquipmentDb.GetOrCreate(ResPathGen.Create(resource, ResPath.EType.ClientResource));
		}

		protected GameEntityHandle CreateAbility(GameEntityHandle owner, string[] resource)
		{
			var handle = abilityCollection.SpawnFor(ResPathGen.Create(resource, ResPath.EType.MasterServer), owner);
			GameWorld.AssureComponents(handle, stackalloc[] { AsComponentType<SimulationAuthority>() });

			return handle;
		}
	}
}
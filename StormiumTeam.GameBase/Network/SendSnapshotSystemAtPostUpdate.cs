using GameHost.Core.Ecs;
using GameHost.Core.Ecs.Passes;
using GameHost.Revolution.NetCode.LLAPI.Systems;

namespace StormiumTeam.GameBase.Network
{
	public class SendSnapshotSystemAtPostUpdate : AppSystem, IPostUpdateSimulationPass
	{
		private SendSnapshotSystem sendSnapshotSystem;
		
		public SendSnapshotSystemAtPostUpdate(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref sendSnapshotSystem);
		}

		public void OnAfterSimulationUpdate()
		{
			sendSnapshotSystem.Enabled = true;
			if (!sendSnapshotSystem.CanUpdate())
				return;

			(sendSnapshotSystem as IUpdatePass).OnUpdate();
			sendSnapshotSystem.Enabled = false;
		}
	}
}
using GameHost.Core.Ecs;
using GameHost.Core.Ecs.Passes;
using GameHost.Revolution.NetCode.LLAPI.Systems;

namespace StormiumTeam.GameBase
{
	public interface IPreUpdateSimulationPass
	{
		void OnBeforeSimulationUpdate();

		public class RegisterPass : PassRegisterBase<IPreUpdateSimulationPass>
		{
			protected override void OnTrigger()
			{ 
				foreach (var pass in GetObjects())
				{
					if (pass is IUpdatePass updatePass && !updatePass.CanUpdate())
						continue;

					pass.OnBeforeSimulationUpdate();
				}
			}
		}
	}

	public interface IUpdateSimulationPass
	{
		void OnSimulationUpdate();

		public class RegisterPass : PassRegisterBase<IUpdateSimulationPass>
		{
			protected override void OnTrigger()
			{
				foreach (var pass in GetObjects())
				{
					if (pass is IUpdatePass updatePass && !updatePass.CanUpdate())
						continue;
					pass.OnSimulationUpdate();
				}
			}
		}
	}

	public interface IPostUpdateSimulationPass
	{
		void OnAfterSimulationUpdate();

		public class RegisterPass : PassRegisterBase<IPostUpdateSimulationPass>
		{
			protected override void OnTrigger()
			{
				foreach (var pass in GetObjects())
				{
					if (pass is IUpdatePass updatePass && !updatePass.CanUpdate())
						continue;
					pass.OnAfterSimulationUpdate();
				}
			}
		}
	}

	public interface IAfterSnapshotDataPass
	{
		void AfterSnapshotData();

		public class RegisterPass : PassRegisterBase<IAfterSnapshotDataPass>
		{
			public RegisterPass()
			{
				ManualTrigger = true;
			}

			protected override void OnTrigger()
			{
				foreach (var pass in GetObjects())
				{
					if (pass is IUpdatePass updatePass && !updatePass.CanUpdate())
						continue;
					pass.AfterSnapshotData();
				}
			}
		}

		public class AddPass : AppSystem
		{
			public AddPass(WorldCollection collection) : base(collection)
			{
				var pass = new RegisterPass();
				collection.DefaultSystemCollection.AddPass(pass, null, null);

				AddDisposable(collection.Mgr.Subscribe((in OnSnapshotReceivedMessage msg) => { pass.Trigger(); }));
			}
		}
	}

	/// <summary>
	/// A <see cref="IGameEventPass"/> should be called manually by a gamemode.
	/// </summary>
	public interface IGameEventPass
	{
		void OnGameEventPass();

		public class RegisterPass : PassRegisterBase<IGameEventPass>
		{
			public RegisterPass()
			{
				// temporary until we have a real implement to call it from client and server
				ManualTrigger = true;
			}

			protected override void OnTrigger()
			{
				foreach (var pass in GetObjects())
				{
					if (pass is IUpdatePass updatePass && !updatePass.CanUpdate())
						continue;
					pass.OnGameEventPass();
				}
			}
		}
	}
}
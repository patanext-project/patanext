using GameHost.Core.Ecs.Passes;

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
}
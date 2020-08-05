using GameHost.Core.Ecs.Passes;

namespace PataNext.Module.Simulation.Passes
{
	public interface IAbilitySimulationPass
	{
		void OnAbilitySimulationPass();

		public class RegisterPass : PassRegisterBase<IAbilitySimulationPass>
		{
			protected override void OnTrigger()
			{
				foreach (var pass in GetObjects())
				{
					if (pass is IUpdatePass updatePass && !updatePass.CanUpdate())
						continue;

					pass.OnAbilitySimulationPass();
				}
			}
		}
	}
}
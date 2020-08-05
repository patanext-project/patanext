using GameHost.Core.Ecs.Passes;

namespace PataNext.Module.Simulation.Passes
{
	public interface IAbilityPreSimulationPass
	{
		void OnAbilityPreSimulationPass();

		public class RegisterPass : PassRegisterBase<IAbilityPreSimulationPass>
		{
			protected override void OnTrigger()
			{
				foreach (var pass in GetObjects())
				{
					if (pass is IUpdatePass updatePass && !updatePass.CanUpdate())
						continue;

					pass.OnAbilityPreSimulationPass();
				}
			}
		}
	}
}
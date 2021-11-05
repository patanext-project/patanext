using GameHost.Core.Ecs.Passes;

namespace PataNext.Module.Simulation.Passes
{
	public interface IRhythmEngineSimulationPass
	{
		void OnRhythmEngineSimulationPass();

		public class RegisterPass : PassRegisterBase<IRhythmEngineSimulationPass>
		{
			protected override void OnTrigger()
			{
				foreach (var pass in GetObjects())
				{
					if (pass is IUpdatePass updatePass && !updatePass.CanUpdate())
						continue;
					pass.OnRhythmEngineSimulationPass();
				}
			}
		}
	}
}
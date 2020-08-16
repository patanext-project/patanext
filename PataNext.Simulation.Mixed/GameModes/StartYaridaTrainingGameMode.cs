using GameHost.Core.Ecs;

namespace PataNext.Module.Simulation.GameModes
{
	public class StartYaridaTrainingGameMode : AppSystem
	{
		private YaridaTrainingGameMode trainingGameMode;
		private int                    frame;

		private const int targetFrame = 10;
		
		public StartYaridaTrainingGameMode(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref trainingGameMode);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (frame >= 0 && frame++ > targetFrame)
			{
				frame = -1;
				trainingGameMode.Start(64);
			}
		}
	}
}
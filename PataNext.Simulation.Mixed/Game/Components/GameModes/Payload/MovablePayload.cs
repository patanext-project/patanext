using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GameModes.Payload
{
	public struct MovablePayload : IComponentData
	{
		public float CaptureRadius;
		public float CurrentSpeed;
		public float SpeedFactor;
	}
}
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Special
{
	public struct UnitBodyCollider : IComponentData
	{
		public float Width;
		public float Height;
		public float Scale;

		public UnitBodyCollider(float width, float height)
		{
			Width  = width;
			Height = height;
			Scale  = 1;
		}
	}
}
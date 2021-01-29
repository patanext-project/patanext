using System.Numerics;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Special
{
	public struct GreatShield : IComponentData
	{
		public bool  ForceScale;
		public float Scale;

		public bool    ForceVertexPosition;
		public Vector2 VertBottom, VertTop;
	}
}
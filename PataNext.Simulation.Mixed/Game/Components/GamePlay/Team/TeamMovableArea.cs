using System;
using GameHost.Simulation.TabEcs.Interfaces;
using StormiumTeam.GameBase;

namespace PataNext.Module.Simulation.Components.GamePlay.Team
{
	public struct TeamMovableArea : IComponentData
	{
		public bool IsValid;

		public float Left;
		public float Right;

		public float Center => MathUtils.LerpNormalized(Left, Right, 0.5f);
		public float Size   => MathF.Abs(Left - Right) * 0.5f;
	}
}
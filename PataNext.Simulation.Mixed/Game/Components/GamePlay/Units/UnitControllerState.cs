using System.Collections.Specialized;
using System.Numerics;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Units
{
	public struct UnitControllerState : IComponentData
	{
		public BitVector32 ControlOverVelocity;
		public bool        PassThroughEnemies;

		public bool  OverrideTargetPosition;
		public float TargetPosition;

		public Vector3 PreviousPosition;

		public bool ControlOverVelocityX
		{
			get => ControlOverVelocity[1];
			set => ControlOverVelocity[1] = value;
		}

		public bool ControlOverVelocityY
		{
			get => ControlOverVelocity[2];
			set => ControlOverVelocity[2] = value;
		}

		public class Register : RegisterGameHostComponentData<UnitControllerState>
		{
		}
	}
}
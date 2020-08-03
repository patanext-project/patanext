using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Module.Simulation.Components.Roles;

namespace PataNext.Module.Simulation.Components.GamePlay.Units
{
	/// <summary>
	///     An Unit (<see cref="UnitDescription"/>) with rhythm abilities should possess a Target with an offset.
	///     Even if it's in MultiPlayer, the unit has a target.
	/// </summary>
	///
	/// <remarks>
	///		The offset help to differentiate where the Unit should be positioned compared to other units (eg: current kit)
	/// </remarks>
	public struct UnitTargetOffset : IComponentData
	{
		public float Value;

		public class Register : RegisterGameHostComponentData<UnitTargetOffset>
		{
		}
	}
}
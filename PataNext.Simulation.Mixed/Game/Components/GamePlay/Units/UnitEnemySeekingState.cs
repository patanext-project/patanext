using System;
using System.Numerics;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Units
{
	public struct UnitEnemySeekingState : IComponentData
	{
		public GameEntity Enemy;
		public float      RelativeDistance;
		public float      SelfDistance;

		public class Register : RegisterGameHostComponentData<UnitEnemySeekingState>
		{
		}
	}

	public struct UnitWeakPoint : IComponentBuffer
	{
		public Vector3 Value;

		public UnitWeakPoint(Vector3 value) => Value = value;
	}

	public static class UnitWeakPointExtensions
	{
		public static (Vector3 pos, float dist) GetNearest(this ComponentBuffer<UnitWeakPoint> buffer, in Vector3 local)
		{
			var result = (Vector3.Zero, dist: -1f);
			foreach (var point in buffer)
			{
				var newDist = Vector3.Distance(point.Value, local);
				if (newDist < result.dist || result.dist < 0)
					result = (point.Value, newDist);
			}
			return result;
		} 

		public static bool TryGetNearest(this ComponentBuffer<UnitWeakPoint> buffer, in Vector3 local, out (Vector3 pos, float dist) result)
		{
			result = GetNearest(buffer, local);
			return result.dist >= 0;
		}
	}
}
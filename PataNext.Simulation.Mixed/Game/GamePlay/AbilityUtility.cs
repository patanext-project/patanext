using System;
using System.Numerics;
using GameHost.Simulation.Utility.InterTick;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using StormiumTeam.GameBase;

namespace PataNext.Module.Simulation.Game.GamePlay
{
	public static class AbilityUtility
	{
		public struct GetTargetVelocityParameters
		{
			public Vector3       TargetPosition;
			public Vector3       PreviousPosition;
			public Vector3       PreviousVelocity;
			public UnitPlayState PlayState;
			public float         Acceleration;
			public float         Delta;
		}

		public static float GetTargetVelocityX(GetTargetVelocityParameters param, float deaccel_distance = -1, float deaccel_distance_max = -1)
		{
			var speed = MathHelper.LerpNormalized(Math.Abs(param.PreviousVelocity.X),
				param.PlayState.MovementAttackSpeed,
				param.PlayState.GetAcceleration() * param.Acceleration * param.Delta);

			if (deaccel_distance >= 0)
			{
				var dist = MathHelper.Distance(param.TargetPosition.X, param.PreviousPosition.X);
				if (dist > deaccel_distance && dist < deaccel_distance_max)
				{
					speed *= MathHelper.UnlerpNormalized(deaccel_distance, deaccel_distance_max, dist);
					speed =  Math.Max(speed, param.Delta);
				}
			}

			var newPosX = MathHelper.MoveTowards(param.PreviousPosition.X, param.TargetPosition.X, speed * param.Delta);

			return (newPosX - param.PreviousPosition.X) / param.Delta;
		}
	}
}
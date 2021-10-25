using System;
using System.Numerics;

namespace PataNext.Module.Simulation.Game.GamePlay
{
    public static class PredictTrajectory 
    {
        public static Vector2 Simple(Vector2 start, Vector2 velocity, Vector2 gravity, float delta = 0.05f, int iteration = 50, float yLimit = 0)
		{
			for (var i = 0; i < iteration; i++)
			{
				velocity += gravity * delta;
				start += velocity * delta;
				
				if (yLimit >= start.Y)
					break;
			}

			return start;
		}

		public static Vector2 GetDisplacement(Vector2 velocity, Vector2 gravity, float time)
		{
			return velocity * time + (gravity * MathF.Pow(time, 2)) / 2;
		}
    }
}
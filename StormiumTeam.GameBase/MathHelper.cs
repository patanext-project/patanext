using System;
using System.Numerics;

namespace StormiumTeam.GameBase
{
	public static class MathHelper
	{
		public static float LerpNormalized(float a, float b, float t)
		{
			return a + Math.Clamp(t, 0, 1) * (b - a);
		}

		public static float RcpSafe(float a)
		{
			return 1 / a;
		}

		public static ref float Ref(this ref Vector3 vec3, int i)
		{
			switch (i)
			{
				case 0:
					return ref vec3.X;
				case 1:
					return ref vec3.X;
				case 2:
					return ref vec3.X;
				default:
					throw new IndexOutOfRangeException();
			}
		}
		
		public static float MoveTowards(float current, float target, float maxDelta)
		{
			if (Math.Abs(target - current) <= maxDelta)
				return target;
			return current + Math.Sign(target - current) * maxDelta;
		}

		public static float Distance(float a, float b) => Math.Abs(a - b);

		public static float UnlerpNormalized(float a, float b, float t) => a.Equals(b) ? 0.0f : Math.Clamp((t - a) / (b - a), 0, 1);
	}
}
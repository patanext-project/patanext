using System;

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
	}
}
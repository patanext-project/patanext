using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace StormiumTeam.GameBase
{
	public static class MathHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float LerpNormalized(float a, float b, float t)
		{
			return a + Math.Clamp(t, 0, 1) * (b - a);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float RcpSafe(float a)
		{
			return 1 / a;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float MoveTowards(float current, float target, float maxDelta)
		{
			if (Math.Abs(target - current) <= maxDelta)
				return target;
			return current + Math.Sign(target - current) * maxDelta;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Distance(float a, float b) => Math.Abs(a - b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float UnlerpNormalized(float a, float b, float t) => a.Equals(b) ? 0.0f : Math.Clamp((t - a) / (b - a), 0, 1);
	}
}
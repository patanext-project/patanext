using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using BepuUtilities;

namespace StormiumTeam.GameBase
{
	public static class MathUtils
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
		public static float DivSafe(float x, float y)
		{
			if (x == 0 || y == 0)
				return 0;
			return x / y;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int DivSafe(int x, int y)
		{
			if (x == 0 || y == 0)
				return 0;
			return x / y;
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
		public static Vector2 XY(this Vector3 vec3) => new Vector2(vec3.X, vec3.Y);

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 ToAngles(Quaternion quaternion)
		{
			//return Vector3.Normalize(new Vector3 {X=quaternion.X, Y=quaternion.Y, Z=quaternion.Z});
			return Vector3.Transform(Vector3.UnitY, quaternion);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 ToAngles(Quaternion quaternion, Vector3 up)
		{
			return Vector3.Transform(up, quaternion);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternion FromDirection(Vector3 vector)
		{
			QuaternionEx.GetQuaternionBetweenNormalizedVectors(Vector3.UnitY, vector, out var quat);
			return quat;
		}
	}
}
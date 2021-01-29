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
		public static double LerpNormalized(double a, double b, float t)
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

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Sign(float value)
		{
			if (value.Equals(float.NaN))
				return 0;

			return Math.Sign(value);
		}

		public static Vector3 NormalizeSafe(Vector3 vector)
		{
			var normalized = Vector3.Normalize(vector);
			if (normalized.Length().Equals(float.NaN))
				return Vector3.Zero;
			return normalized;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float MoveTowards(float current, float target, float maxDelta)
		{
			if (Math.Abs(target - current) <= maxDelta)
				return target;
			return current + Sign(target - current) * maxDelta;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDelta)
		{
			return new(
				MoveTowards(current.X, target.X, maxDelta),
				MoveTowards(current.Y, target.Y, maxDelta),
				MoveTowards(current.Z, target.Z, maxDelta)
			);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Distance(float a, float b) => Math.Abs(a - b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float UnlerpNormalized(float a, float b, float t) => a.Equals(b) ? 0.0f : Math.Clamp((t - a) / (b - a), 0, 1);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Unlerp(float a, float b, float t) => a.Equals(b) ? 0.0f : (t - a) / (b - a);

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

		public static float Angle(Vector3 from, Vector3 to)
		{
			var denominator = (float) Math.Sqrt(from.LengthSquared() * to.LengthSquared());
			if (denominator < 1e-15F)
				return 0F;

			var dot = Math.Clamp(Vector3.Dot(from, to) / denominator, -1F, 1F);
			return MathHelper.ToDegrees((float) Math.Acos(dot));
		}

		public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
		{
			var sqrmag = vector.LengthSquared();
			if (sqrmag > maxLength * maxLength)
			{
				var mag = (float) Math.Sqrt(sqrmag);
				//these intermediate variables force the intermediate result to be
				//of float precision. without this, the intermediate result can be of higher
				//precision, which changes behavior.
				var normalizedX = vector.X / mag;
				var normalizedY = vector.Y / mag;
				var normalizedZ = vector.Z / mag;
				return new Vector3(normalizedX * maxLength,
					normalizedY * maxLength,
					normalizedZ * maxLength);
			}

			return vector;
		}
	}
}
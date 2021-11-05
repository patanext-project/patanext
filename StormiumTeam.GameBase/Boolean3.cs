using System;
using System.Runtime.InteropServices;

namespace StormiumTeam.GameBase
{
	[Serializable]
	public struct Boolean3 : IEquatable<Boolean3>
	{
		[MarshalAs(UnmanagedType.U1)]
		public bool X;

		[MarshalAs(UnmanagedType.U1)]
		public bool Y;

		[MarshalAs(UnmanagedType.U1)]
		public bool Z;

		public Boolean3(bool value)
		{
			X = Y = Z = value;
		}

		public bool Equals(Boolean3 other)
		{
			return X == other.X && Y == other.Y && Z == other.Z;
		}

		public override bool Equals(object obj)
		{
			return obj is Boolean3 other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(X, Y, Z);
		}

		public static bool operator ==(Boolean3 left, Boolean3 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Boolean3 left, Boolean3 right)
		{
			return !left.Equals(right);
		}
	}
}
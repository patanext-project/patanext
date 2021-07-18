using System;
using GameHost.Native.Char;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.Utility.Resource.Components;
using GameHost.Simulation.Utility.Resource.Interfaces;

namespace PataNext.Module.Simulation.Resources
{
	public readonly struct UnitRoleResource : IGameResourceDescription, IEquatable<UnitRoleResource>
	{
		public readonly CharBuffer64 Value;

		public class Register : RegisterGameHostComponentData<UnitRoleResource>
		{
		}

		public UnitRoleResource(CharBuffer64 value) => Value = value;
		public UnitRoleResource(string       value) => Value = CharBufferUtility.Create<CharBuffer64>(value);

		public bool Equals(UnitRoleResource other)
		{
			return Value.Equals(other.Value);
		}

		public override bool Equals(object obj)
		{
			return obj is UnitRoleResource other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}
}
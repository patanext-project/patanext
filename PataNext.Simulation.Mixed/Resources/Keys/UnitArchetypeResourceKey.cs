using System;
using GameHost.Native.Char;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.Utility.Resource.Components;

namespace PataNext.Module.Simulation.Resources.Keys
{
	public readonly struct UnitArchetypeResourceKey : IGameResourceKeyDescription, IEquatable<UnitArchetypeResourceKey>
	{
		public readonly CharBuffer64 Value;

		public class Register : RegisterGameHostComponentData<GameResourceKey<UnitArchetypeResourceKey>>
		{
		}

		public UnitArchetypeResourceKey(CharBuffer64 value) => Value = value;
		public UnitArchetypeResourceKey(string       value) => Value = CharBufferUtility.Create<CharBuffer64>(value);

		public bool Equals(UnitArchetypeResourceKey other)
		{
			return Value.Equals(other.Value);
		}

		public override bool Equals(object obj)
		{
			return obj is UnitArchetypeResourceKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}
}
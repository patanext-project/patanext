using System;
using GameHost.Native;
using GameHost.Native.Char;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.Utility.Resource.Components;

namespace PataNext.Module.Simulation.Resources.Keys
{
	public readonly struct UnitKitResourceKey : IGameResourceKeyDescription, IEquatable<UnitKitResourceKey>
	{
		public readonly CharBuffer64 Value;

		public class Register : RegisterGameHostComponentData<GameResourceKey<UnitKitResourceKey>>
		{
		}

		public UnitKitResourceKey(CharBuffer64 value) => Value = value;
		public UnitKitResourceKey(string       value) => Value = CharBufferUtility.Create<CharBuffer64>(value);
		
		public bool Equals(UnitKitResourceKey other)
		{
			return Value.Equals(other.Value);
		}

		public override bool Equals(object obj)
		{
			return obj is UnitKitResourceKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}
}
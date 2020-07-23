using System;
using GameHost.Native;
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
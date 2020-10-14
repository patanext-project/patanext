using System;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource.Components;

namespace PataNext.Module.Simulation.Resources.Keys
{
	public readonly struct RhythmCommandResourceKey : IGameResourceKeyDescription, IEquatable<RhythmCommandResourceKey>
	{
		public readonly ComponentType Identifier;

		public class Register : RegisterGameHostComponentData<GameResourceKey<RhythmCommandResourceKey>>
		{
		}

		public RhythmCommandResourceKey(ComponentType identifier)
		{
			Identifier   = identifier;
		}

		public bool Equals(RhythmCommandResourceKey other)
		{
			return Identifier.Equals(other.Identifier);
		}

		public override bool Equals(object obj)
		{
			return obj is RhythmCommandResourceKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Identifier.GetHashCode();
		}
	}
}
using System;
using GameHost.Native;
using GameHost.Native.Char;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.Utility.Resource.Components;

namespace PataNext.Module.Simulation.Resources.Keys
{
	public readonly struct RhythmCommandResourceKey : IGameResourceKeyDescription, IEquatable<RhythmCommandResourceKey>
	{
		public readonly CharBuffer64 Value;

		public class Register : RegisterGameHostComponentData<GameResourceKey<RhythmCommandResourceKey>>
		{
		}
		
		public RhythmCommandResourceKey(CharBuffer64 value) => Value = value;
		public RhythmCommandResourceKey(string       value) => Value = CharBufferUtility.Create<CharBuffer64>(value);
		
		public static implicit operator RhythmCommandResourceKey(string value) => new RhythmCommandResourceKey(value);

		public bool Equals(RhythmCommandResourceKey other)
		{
			return Value.Equals(other.Value);
		}

		public override bool Equals(object obj)
		{
			return obj is RhythmCommandResourceKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}
}
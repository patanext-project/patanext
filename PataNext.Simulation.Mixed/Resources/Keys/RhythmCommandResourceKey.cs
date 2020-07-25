using System;
using GameHost.Native;
using GameHost.Native.Char;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.Utility.Resource.Components;

namespace PataNext.Module.Simulation.Resources.Keys
{
	public readonly struct RhythmCommandResourceKey : IGameResourceKeyDescription, IEquatable<RhythmCommandResourceKey>
	{
		public readonly CharBuffer64 Identifier;
		public readonly int          BeatDuration;

		public class Register : RegisterGameHostComponentData<GameResourceKey<RhythmCommandResourceKey>>
		{
		}

		public RhythmCommandResourceKey(CharBuffer64 identifier, int beatDuration)
		{
			Identifier   = identifier;
			BeatDuration = beatDuration;
		}

		public RhythmCommandResourceKey(string value, int beatDuration) : this(CharBufferUtility.Create<CharBuffer64>(value), beatDuration)
		{
		}

		public static implicit operator RhythmCommandResourceKey((string value, int beatDuration) tuple) => new RhythmCommandResourceKey(tuple.value, tuple.beatDuration);

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
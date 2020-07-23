using System;
using GameHost.Native;
using GameHost.Native.Char;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.Utility.Resource.Components;

namespace PataNext.Module.Simulation.Resources.Keys
{
	public readonly struct UnitAttachmentResourceKey : IGameResourceKeyDescription, IEquatable<UnitAttachmentResourceKey>
	{
		public readonly CharBuffer64 Value;

		public class Register : RegisterGameHostComponentData<GameResourceKey<UnitAttachmentResourceKey>>
		{
		}
		
		public UnitAttachmentResourceKey(CharBuffer64 value) => Value = value;
		public UnitAttachmentResourceKey(string       value) => Value = CharBufferUtility.Create<CharBuffer64>(value);
		
		public static implicit operator UnitAttachmentResourceKey(string value) => new UnitAttachmentResourceKey(value);

		public bool Equals(UnitAttachmentResourceKey other)
		{
			return Value.Equals(other.Value);
		}

		public override bool Equals(object obj)
		{
			return obj is UnitAttachmentResourceKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}
}
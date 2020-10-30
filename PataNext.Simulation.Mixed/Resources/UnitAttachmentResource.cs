using System;
using GameHost.Native.Char;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.Utility.Resource.Components;
using GameHost.Simulation.Utility.Resource.Interfaces;

namespace PataNext.Module.Simulation.Resources
{
	public readonly struct UnitAttachmentResource : IGameResourceDescription, IEquatable<UnitAttachmentResource>
	{
		public readonly CharBuffer64 Value;

		public class Register : RegisterGameHostComponentData<UnitAttachmentResource>
		{
		}
		
		public UnitAttachmentResource(CharBuffer64 value) => Value = value;
		public UnitAttachmentResource(string       value) => Value = CharBufferUtility.Create<CharBuffer64>(value);
		
		public static implicit operator UnitAttachmentResource(string value) => new UnitAttachmentResource(value);

		public bool Equals(UnitAttachmentResource other)
		{
			return Value.Equals(other.Value);
		}

		public override bool Equals(object obj)
		{
			return obj is UnitAttachmentResource other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}
}
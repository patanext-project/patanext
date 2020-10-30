using System;
using GameHost.Native.Char;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.Utility.Resource.Components;
using GameHost.Simulation.Utility.Resource.Interfaces;

namespace PataNext.Module.Simulation.Resources
{
	public readonly struct EquipmentResource : IGameResourceDescription, IEquatable<EquipmentResource>
	{
		public readonly CharBuffer64 Value;

		public class Register : RegisterGameHostComponentData<EquipmentResource>
		{
		}

		public EquipmentResource(CharBuffer64 value) => Value = value;
		public EquipmentResource(string       value) => Value = CharBufferUtility.Create<CharBuffer64>(value);

		public static implicit operator EquipmentResource(string value) => new EquipmentResource(value);


		public bool Equals(EquipmentResource other)
		{
			return Value.Equals(other.Value);
		}

		public override bool Equals(object obj)
		{
			return obj is EquipmentResource other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}
}
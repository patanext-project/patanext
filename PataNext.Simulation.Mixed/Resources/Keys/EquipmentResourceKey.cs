using System;
using GameHost.Native.Char;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.Utility.Resource.Components;

namespace PataNext.Module.Simulation.Resources.Keys
{
	public readonly struct EquipmentResourceKey : IGameResourceKeyDescription, IEquatable<EquipmentResourceKey>
	{
		public readonly CharBuffer64 Value;

		public class Register : RegisterGameHostComponentData<GameResourceKey<EquipmentResourceKey>>
		{
		}
		
		public EquipmentResourceKey(CharBuffer64 value) => Value = value;
		public EquipmentResourceKey(string       value) => Value = CharBufferUtility.Create<CharBuffer64>(value);
		
		public static implicit operator EquipmentResourceKey(string value) => new EquipmentResourceKey(value);


		public bool Equals(EquipmentResourceKey other)
		{
			return Value.Equals(other.Value);
		}

		public override bool Equals(object obj)
		{
			return obj is EquipmentResourceKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}
}
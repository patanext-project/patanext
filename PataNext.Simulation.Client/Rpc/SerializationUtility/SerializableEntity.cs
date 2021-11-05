using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DefaultEcs;
using Newtonsoft.Json;

namespace PataNext.Game.Rpc.SerializationUtility
{
	[StructLayout(LayoutKind.Explicit)]
	public struct SerializableEntity : IEquatable<SerializableEntity>
	{
		public static implicit operator SerializableEntity(Entity entity)
		{
			return Unsafe.As<Entity, SerializableEntity>(ref entity);
		}
		
		public static implicit operator Entity(SerializableEntity entity)
		{
			return Unsafe.As<SerializableEntity, Entity>(ref entity);
		}
		
		[FieldOffset(0)]
		public short Version;

		[FieldOffset(2)]
		public short WorldId;

		[FieldOffset(4)]
		public int EntityId;

		public bool Equals(SerializableEntity other)
		{
			return Version == other.Version && WorldId == other.WorldId && EntityId == other.EntityId;
		}

		public override bool Equals(object obj)
		{
			return obj is SerializableEntity other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Version.GetHashCode();
				hashCode = (hashCode * 397) ^ WorldId.GetHashCode();
				hashCode = (hashCode * 397) ^ EntityId;
				return hashCode;
			}
		}
	}
}
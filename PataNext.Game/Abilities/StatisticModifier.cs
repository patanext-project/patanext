using System;
using GameHost.Native;
using GameHost.Native.Fixed;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Game.Abilities
{
	public struct StatisticModifier
	{
		// The buffer should be equal to

		public FixedBuffer256<StatusEffectModifier> StatusEffects;

		public float Attack;
		public float Defense;

		public float ReceiveDamage;

		public float MovementSpeed;
		public float MovementAttackSpeed;
		public float MovementReturnSpeed;
		public float AttackSpeed;

		public float AttackSeekRange;

		public float Weight;
		public float Knockback;

		public static readonly StatisticModifier Default = new StatisticModifier
		{
			Attack  = 1,
			Defense = 1,

			ReceiveDamage = 1,

			MovementSpeed       = 1,
			MovementAttackSpeed = 1,
			MovementReturnSpeed = 1,
			AttackSpeed         = 1,

			AttackSeekRange = 1,

			Weight    = 1,
			Knockback = 1,
		};

		private static int FindIndex(Span<StatusEffectModifier> list, StatusEffect type)
		{
			var length = list.Length;
			for (var i = 0; i != length; i++)
			{
				if (list[i].Type == type)
					return i;
			}

			return -1;
		}

		public static bool TryGetEffect(ref Span<StatusEffectModifier> fixedBuffer, StatusEffect type, out StatusEffectModifier modifier)
		{
			var index = FindIndex(fixedBuffer, type);
			if (index < 0)
			{
				modifier = default;
				return false;
			}

			modifier = fixedBuffer[index];
			return true;
		}

		public static ref StatusEffectModifier SetEffectRef<TBuffer>(ref TBuffer fixedBuffer, StatusEffect type)
			where TBuffer : struct, IFixedBuffer<StatusEffectModifier>
		{
			var index = FindIndex(fixedBuffer.Span, type);
			if (index >= 0) 
				return ref fixedBuffer.Span[index];
			
			var length = fixedBuffer.Length;
			fixedBuffer.Add(new StatusEffectModifier {Type = type});
			return ref fixedBuffer.Span[length];
		}
	}

	public struct StatusEffectModifier : IEquatable<StatusEffectModifier>
	{
		public StatusEffect Type;
		public float        Power;
		public float        RegenPerSecond;
		public float        ReceiveImmunity;
		public float        ReceivePower;

		public bool Equals(StatusEffectModifier other)
		{
			return UnsafeUtility.SameData(ref this, ref other);
		}

		public override bool Equals(object obj)
		{
			return obj is StatusEffectModifier other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Type, Power, RegenPerSecond, ReceiveImmunity);
		}

		public static bool operator ==(StatusEffectModifier left, StatusEffectModifier right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(StatusEffectModifier left, StatusEffectModifier right)
		{
			return !left.Equals(right);
		}
	}
}
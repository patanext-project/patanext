using System;
using GameHost.Native;
using GameHost.Native.Fixed;

namespace PataNext.Game.Abilities
{
	public struct StatisticModifier
	{
		// The buffer should be equal to
		
		public FixedBuffer128<StatusEffectModifier> OffensiveEffects;
		public FixedBuffer128<StatusEffectModifier> DefensiveEffects;
		
		public float Attack;
		public float Defense;

		public float ReceiveDamage;

		public float MovementSpeed;
		public float MovementAttackSpeed;
		public float MovementReturnSpeed;
		public float AttackSpeed;

		public float AttackSeekRange;

		public float Weight;

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

			Weight = 1,
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
				modifier = new StatusEffectModifier {Type = type, Multiplier = 1};
				return false;
			}

			modifier = fixedBuffer[index];
			return true;
		}

		public static void SetEffect<TBuffer>(ref TBuffer fixedBuffer, StatusEffect type, float multiplier)
			where TBuffer : struct, IFixedBuffer<StatusEffectModifier>
		{
			var index = FindIndex(fixedBuffer.Span, type);
			if (index < 0)
				fixedBuffer.Add(new StatusEffectModifier {Type = type, Multiplier = multiplier});
			else
				fixedBuffer.Span[index] = new StatusEffectModifier {Type = type, Multiplier = multiplier};
		}
	}
	
	public struct StatusEffectModifier : IEquatable<StatusEffectModifier>
	{
		public StatusEffect Type;
		public float        Multiplier;

		public bool Equals(StatusEffectModifier other)
		{
			return Type.Equals(other.Type) && Multiplier.Equals(other.Multiplier);
		}

		public override bool Equals(object obj)
		{
			return obj is StatusEffectModifier other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Type, Multiplier);
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
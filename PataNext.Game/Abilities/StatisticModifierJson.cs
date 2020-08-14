using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Game.Abilities
{
public static class StatisticModifierJson
	{
		public static string Convert(StatisticModifier modifier)
		{
			return string.Empty;
		}
		
		/// <summary>
		/// don't mute the dictionary!
		/// </summary>
		/// <returns></returns>
		public static unsafe void FromMap(ref Dictionary<string, StatisticModifier> hashMap, string json)
		{
			if (json == null)
				return;
			
			using var stream   = new MemoryStream(Encoding.UTF8.GetBytes(json.ToLower()));
			using var reader = JsonDocument.Parse(stream);
			{
				var root = reader.RootElement;
				if (!root.TryGetProperty("modifiers", out var modifierProp))
					return;
				
				var modifiers = modifierProp.EnumerateObject();
				foreach (var prop in modifiers)
				{
					if (prop.Value.ValueKind != JsonValueKind.Object)
						continue;

					var mod = StatisticModifier.Default;
					Deserialize(ref mod, prop.Value);
					hashMap[prop.Name] = mod;
				}
			}
		}

		private static void Deserialize(ref StatisticModifier modifier, JsonElement element)
		{
			void update(ref float original, string member)
			{
				if (element.TryGetProperty(member, out var value))
					original = (float) value.GetDouble();
			}

			update(ref modifier.Attack, "attack");
			update(ref modifier.Defense, "defense");

			update(ref modifier.ReceiveDamage, "receive_damage");

			update(ref modifier.MovementSpeed, "movement_speed");
			update(ref modifier.MovementAttackSpeed, "movement_attack_speed");
			update(ref modifier.MovementReturnSpeed, "movement_return_speed");
			update(ref modifier.AttackSpeed, "attack_speed");

			update(ref modifier.AttackSeekRange, "attack_seek_range");

			update(ref modifier.Weight, "weight");
		}

		/*public static StatisticModifier From(string json)
		{
			var modifier = StatisticModifier.Default;
			using (var reader = new SerializedObjectReader(json.ToLower()))
			{
				var entity = reader.ReadObject();
				Deserialize(ref modifier, entity);
			}

			return modifier;
		}*/
	}
}
using System.IO;
using System.Text;
using System.Text.Json;
using PataNext.Game.Modules.Abilities.Components;
using revecs.Core;

namespace PataNext.Game.Modules.Abilities;

// TODO: We need to remake status effects (storing it into the StatisticsModifier struct is a bad idea)

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
	public static unsafe void FromMap(Dictionary<string, StatisticModifier> hashMap, string json)
	{
		if (json == null)
			return;

		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json.ToLower()));
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
		bool update(ref float original, string member)
		{
			if (element.TryGetProperty(member, out var value))
			{
				original = (float) value.GetDouble();
				return true;
			}

			return false;
		}

		/*void update_status(ref StatusEffectModifier statusEffectModifier, JsonElement obj)
		{
			statusEffectModifier.Power = 1;
			statusEffectModifier.ReceiveImmunity = 1;
			statusEffectModifier.ReceivePower = 1;
			statusEffectModifier.RegenPerSecond = 1;

			JsonElement value;
			if (obj.TryGetProperty("power", out value))
				statusEffectModifier.Power = (float) value.GetDouble();
			if (obj.TryGetProperty("receive_immunity", out value))
				statusEffectModifier.ReceiveImmunity = (float) value.GetDouble();
			if (obj.TryGetProperty("receive_power", out value))
				statusEffectModifier.ReceivePower = (float) value.GetDouble();
			if (obj.TryGetProperty("regen", out value))
				statusEffectModifier.RegenPerSecond = (float) value.GetDouble();
		}*/

		update(ref modifier.Attack, "attack");
		update(ref modifier.Defense, "defense");

		update(ref modifier.ReceiveDamage, "receive_damage");

		update(ref modifier.MovementSpeed, "movement_speed");
		update(ref modifier.MovementAttackSpeed, "movement_attack_speed");
		update(ref modifier.MovementReturnSpeed, "movement_return_speed");
		update(ref modifier.AttackSpeed, "attack_speed");

		update(ref modifier.AttackSeekRange, "attack_seek_range");

		update(ref modifier.Weight, "weight");
		update(ref modifier.Knockback, "knockback");

		/*if (element.TryGetProperty("status_global", out var statusObj))
		{
			update_status(
				ref StatisticModifier.SetEffectRef(ref modifier.StatusEffects, gameWorld.AsComponentType<Critical>()),
				statusObj);
			update_status(
				ref StatisticModifier.SetEffectRef(ref modifier.StatusEffects, gameWorld.AsComponentType<KnockBack>()),
				statusObj);
			update_status(
				ref StatisticModifier.SetEffectRef(ref modifier.StatusEffects, gameWorld.AsComponentType<Stagger>()),
				statusObj);
			update_status(
				ref StatisticModifier.SetEffectRef(ref modifier.StatusEffects, gameWorld.AsComponentType<Piercing>()),
				statusObj);
		}*/
	}
}
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;
using PataNext.Game.Abilities;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Utility.Misc;

namespace PataNext.Module.Simulation.Systems
{
	public struct StatusEffectStateBase : IComponentData, IMetadataCustomComponentName
	{
		public StatusEffect Type;
		public float        CurrentResistance;
		public float        CurrentRegenPerSecond;
		public float        CurrentPower;
		public float        CurrentImmunity;
		public float        ReceivedPowerPercentage;

		public float ImmunityExp;

		public string ProvideName(GameWorld gameWorld)
		{
			return nameof(StatusEffectStateBase);
		}

		public void ResetValues()
		{
			CurrentResistance       = default;
			CurrentRegenPerSecond   = default;
			CurrentPower            = default;
			CurrentImmunity         = default;
			ReceivedPowerPercentage = 1;
		}
	}

	public struct StatusEffectSettingsBase : IComponentData, IMetadataCustomComponentName
	{
		public StatusEffect Type;
		public float        Resistance;
		public float        RegenPerSecond;
		public float        Power;
		public float        ImmunityPerAttack;

		public string ProvideName(GameWorld gameWorld)
		{
			return nameof(StatusEffectSettingsBase);
		}
	}

	public class UnitStatusEffectComponentProvider : GameAppSystem
	{
		private ComponentType baseStateType;
		private ComponentType baseSettingsType;

		private Dictionary<StatusEffect, ComponentType> stateMap    = new();
		private Dictionary<StatusEffect, ComponentType> settingsMap = new();

		public UnitStatusEffectComponentProvider([NotNull] WorldCollection collection) : base(collection)
		{
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			baseStateType    = AsComponentType<StatusEffectStateBase>();
			baseSettingsType = AsComponentType<StatusEffectSettingsBase>();

			// We aren't in a performance critical context, so it's fine to call Enum.GetValues()
			// TODO: Optimize it in the future
			foreach (StatusEffect enumType in Enum.GetValues(typeof(StatusEffect)))
			{
				var name = enumType switch
				{
					StatusEffect.Critical => TypeExt.GetFriendlyName(typeof(PataNext.Game.Abilities.Effects.Critical)),
					StatusEffect.KnockBack => TypeExt.GetFriendlyName(typeof(PataNext.Game.Abilities.Effects.KnockBack)),
					StatusEffect.Stagger => TypeExt.GetFriendlyName(typeof(PataNext.Game.Abilities.Effects.Stagger)),
					StatusEffect.Burn => TypeExt.GetFriendlyName(typeof(PataNext.Game.Abilities.Effects.Burn)),
					StatusEffect.Sleep => TypeExt.GetFriendlyName(typeof(PataNext.Game.Abilities.Effects.Sleep)),
					StatusEffect.Freeze => TypeExt.GetFriendlyName(typeof(PataNext.Game.Abilities.Effects.Freeze)),
					StatusEffect.Poison => TypeExt.GetFriendlyName(typeof(PataNext.Game.Abilities.Effects.Poison)),
					StatusEffect.Tumble => TypeExt.GetFriendlyName(typeof(PataNext.Game.Abilities.Effects.Tumble)),
					StatusEffect.Wind => TypeExt.GetFriendlyName(typeof(PataNext.Game.Abilities.Effects.Wind)),
					StatusEffect.Piercing => TypeExt.GetFriendlyName(typeof(PataNext.Game.Abilities.Effects.Piercing)),
					StatusEffect.Silence => TypeExt.GetFriendlyName(typeof(PataNext.Game.Abilities.Effects.Silence)),
					_ => null
				};
				if (name == null)
					continue;

				stateMap[enumType] = GameWorld.RegisterComponent(name + "State",
					new SingleComponentBoard(Unsafe.SizeOf<StatusEffectStateBase>(), 0),
					optionalParentType: AsComponentType<StatusEffectStateBase>());
				settingsMap[enumType] = GameWorld.RegisterComponent(name + "Settings",
					new SingleComponentBoard(Unsafe.SizeOf<StatusEffectSettingsBase>(), 0),
					optionalParentType: AsComponentType<StatusEffectSettingsBase>());
			}
		}

		public void AddStatus(GameEntityHandle handle, StatusEffect type, StatusEffectSettingsBase settings)
		{
			settings.Type = type;

			GameWorld.AddComponent(handle, stateMap[type], new StatusEffectStateBase {Type = type, CurrentResistance = settings.Resistance});
			GameWorld.AddComponent(handle, settingsMap[type], settings);
		}

		public bool HasStatus(GameEntityHandle handle, StatusEffect type)
		{
			return GameWorld.HasComponent(handle, stateMap[type]);
		}

		public ref StatusEffectStateBase GetStatusState(GameEntityHandle handle, StatusEffect type)
		{
			return ref GameWorld.GetComponentData<StatusEffectStateBase>(handle, stateMap[type]);
		}

		public ref StatusEffectSettingsBase GetStatusSettings(GameEntityHandle handle, StatusEffect type)
		{
			return ref GameWorld.GetComponentData<StatusEffectSettingsBase>(handle, settingsMap[type]);
		}
	}
}
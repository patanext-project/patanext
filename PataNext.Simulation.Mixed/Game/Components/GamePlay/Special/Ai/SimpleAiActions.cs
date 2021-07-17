using System;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Special.Ai
{
	public struct SimpleAiActionIndex : IComponentData
	{
		public TimeSpan TimeBeforeNextAbility;
		public int      Index;
	}

	public struct SimpleAiActions : IComponentBuffer
	{
		public enum EType
		{
			Wait,
			Ability
		}

		public EType    Type;
		public TimeSpan Duration;

		public GameEntity AbilityTarget;

		public void SetAbility(GameEntity ability) => SetAbility(ability, TimeSpan.FromSeconds(2));

		public void SetAbility(GameEntity ability, TimeSpan duration)
		{
			Type = EType.Ability;

			AbilityTarget = ability;
			Duration      = duration;
		}

		public void SetWait() => SetWait(TimeSpan.FromSeconds(2));

		public void SetWait(TimeSpan duration)
		{
			Type = EType.Wait;

			Duration = duration;
		}
	}
}
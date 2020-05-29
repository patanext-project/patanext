using System;
using DefaultEcs;

namespace PataponGameHost.RhythmEngine.Components
{
	public static class GameCombo
	{
		public struct Settings
		{
			public int   MaxComboToReachFever;
			public float RequiredScoreStart;
			public float RequiredScoreStep;

			public bool CanEnterFever(int combo, float score)
			{
				return combo > MaxComboToReachFever
				       || (RequiredScoreStart - combo * RequiredScoreStep) < score;
			}
		}

		public struct State
		{
			/// <summary>
			/// Combo count
			/// </summary>
			public int Count;

			/// <summary>
			/// Combo score
			/// </summary>
			public float Score;
		}

		public static void AddToEntity(Entity entity)
		{
			entity.Set(new Settings {MaxComboToReachFever = 9, RequiredScoreStart = 4.0f, RequiredScoreStep = 0.5f});
			entity.Set(new State());
		}

		public static EntityRuleBuilder WithGameCombo(this EntityRuleBuilder ruleBuilder)
		{
			return ruleBuilder.With<Settings>().With<State>();
		}
	}
}
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.RhythmEngine
{
	public static class GameCombo
	{
		public struct Settings : IComponentData
		{
			public int   MaxComboToReachFever;
			public float RequiredScoreStart;
			public float RequiredScoreStep;

			public bool CanEnterFever(int combo, float score)
			{
				return combo > MaxComboToReachFever
				       || (RequiredScoreStart - combo * RequiredScoreStep) < score;
			}
			
			public bool CanEnterFever(State state)
			{
				return CanEnterFever(state.Count, state.Score);
			}

			public class Register : RegisterGameHostComponentData<GameCombo.Settings>
			{
			}
		}

		public struct State : IComponentData
		{
			/// <summary>
			/// Combo count
			/// </summary>
			public int Count;

			/// <summary>
			/// Combo score
			/// </summary>
			public float Score;

			public class Register : RegisterGameHostComponentData<GameCombo.State>
			{
			}
		}

		public static void AddToEntity(GameWorld gameWorld, GameEntity entity)
		{
			gameWorld.AddComponent(entity, new Settings {MaxComboToReachFever = 9, RequiredScoreStart = 4.0f, RequiredScoreStep = 0.4f});
			gameWorld.AddComponent(entity, new State { });
		}
	}
}
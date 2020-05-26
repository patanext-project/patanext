using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.HostSerialization.imp;
using RevolutionSnapshot.Core;
using RevolutionSnapshot.Core.ECS;

namespace PataponGameHost.RhythmEngine.Components
{
	public struct RhythmEngineExecutingCommand : ITransformEntities<RhythmEngineExecutingCommand>
	{
		public Entity Previous;
		public Entity CommandTarget;

		/// <summary>
		/// At which 'activation' beat will the command start?
		/// </summary>
		public int ActivationBeatStart;

		/// <summary>
		/// At which 'activation' beat will the command end?
		/// </summary>
		public int ActivationBeatEnd;

		/// <summary>
		///     Power is associated with beat score, this is a value between 0 and 100.
		/// </summary>
		/// <remarks>
		///     This is not associated at all with fever state, the command will check if there is fever or not on the engine.
		///     The game will check if it can enable hero mode if power is 100.
		/// </remarks>
		public int PowerInteger;

		public bool WaitingForApply;

		/// <summary>
		/// Return a power between a range of [0..1]
		/// </summary>
		public double Power
		{
			get => PowerInteger * 0.01;
			set => PowerInteger = (int) Math.Clamp(value * 100, 0, 100);
		}

		public void TransformFrom(TwoWayDictionary<Entity, Entity> fromToMap, RhythmEngineExecutingCommand other)
		{
			fromToMap.TryGetValue(other.Previous, out Previous);
			fromToMap.TryGetValue(other.CommandTarget, out CommandTarget);
		}

		public override string ToString()
		{
			return $"Target={CommandTarget}, ActiveAt={ActivationBeatStart}, Power={Power:0.00%}";
		}
	}

	public class RhythmEnginePredictedCommandBuffer : List<Entity>
	{
	}
}
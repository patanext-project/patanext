using System;
using System.Collections.Generic;
using System.Linq;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Core.Features.Systems;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Revolution.Snapshot.Systems.Instigators;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Time;
using GameHost.Worlds.Components;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using StormiumTeam.GameBase.SystemBase;
using ZLogger;

namespace StormiumTeam.GameBase.Network
{
	public struct CalculatedNetTime : IComponentData
	{
		public int frame;
		public int interpolatedFrame;
		public int frameDiff;
		
		public float    delta;
		public TimeSpan lastRaw;
		public TimeSpan currentInterpolated;
		public TimeSpan currentSlowed; // slowed time are like interpolated, except it will force on not being ahead
		public TimeSpan diffInterpolated;
		public TimeSpan currentExtrapolated;
	}

	[UpdateAfter(typeof(UpdateDriverSystem))]
	public class InstigatorTimeSystem : AppSystemWithFeature<MultiplayerFeature>
	{
		private ILogger           logger;
		private IManagedWorldTime worldTime;
		private GameWorld         gameWorld;

		public InstigatorTimeSystem([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref logger);
			DependencyResolver.Add(() => ref worldTime);
			DependencyResolver.Add(() => ref gameWorld);
		}

		private CalculatedNetTime ReturnDefault()
		{
			return new()
			{
				lastRaw             = worldTime.Total,
				currentExtrapolated = worldTime.Total,
				currentInterpolated = worldTime.Total,
				delta               = (float) worldTime.Delta.TotalSeconds,
			};
		}

		public (bool defaultTime, CalculatedNetTime) GetTime(int instigatorId)
		{
			if (instigatorId == 0 && Features.Count == 0)
				return (true, ReturnDefault());

			foreach (var (ent, feature) in Features)
			{
				if (feature is ServerFeature && instigatorId == 0)
					return (false, ReturnDefault());

				if (!ent.TryGet(out BroadcastInstigator broadcastInstigator))
					continue;

				if (broadcastInstigator.InstigatorId == instigatorId
				    && broadcastInstigator.Storage.TryGet(out CalculatedNetTime calculatedNetTime))
					return (false, calculatedNetTime);

				if (broadcastInstigator.TryGetClient(instigatorId, out var client)
				    && client.Storage.TryGet(out calculatedNetTime))
					return (false, calculatedNetTime);
			}

			return (true, ReturnDefault());
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			foreach (var (ent, feature) in Features)
			{
				if (!ent.TryGet(out BroadcastInstigator broadcastInstigator))
					continue;

				foreach (var client in broadcastInstigator.clients)
				{
					var storage = client.Storage;
					if (!storage.Has<CalculatedNetTime>())
						storage.Set<CalculatedNetTime>();

					GameEntity gameEntity;
					if (!storage.Has<GameEntity>() || storage.TryGet(out gameEntity) && !gameWorld.Exists(gameEntity))
					{
						storage.Set(gameEntity = gameWorld.Safe(gameWorld.CreateEntity()));

						gameWorld.AddComponent(gameEntity.Handle, gameWorld.AsComponentType<CalculatedNetTime>());
					}

					if (!gameWorld.HasComponent<CalculatedNetTime>(gameEntity.Handle))
						gameWorld.AddComponent(gameEntity.Handle, new CalculatedNetTime());

					ref var calcul = ref storage.Get<CalculatedNetTime>();

					if (storage.TryGet(out List<GameTime> times))
					{
						var lastDelta = default(float);
						foreach (var time in times)
						{
							if (lastDelta.Equals(default))
								lastDelta = time.Delta;

							if (lastDelta.Equals(time.Delta) == false)
							{
								lastDelta = time.Delta;
								logger.ZLogWarning($"Delta Diff");
							}
						}

						if (!lastDelta.Equals(default))
							calcul.delta = lastDelta;

						var interSeconds = calcul.currentInterpolated.TotalSeconds;
						var factor       = 1f;
						var bigDiff      = TimeSpan.Zero;

						if (times.Any())
						{
							var last = times.Last();
							// Big difference, TP to a quart of it

							var diff      = Math.Abs(interSeconds - last.Elapsed);
							var wereAhead = interSeconds > last.Elapsed;

							var framesAdded = 0;
							if (diff > 5)
								interSeconds = MathUtils.LerpNormalized(interSeconds, last.Elapsed, 0.75f);
							if (diff > 3)
								interSeconds = MathUtils.LerpNormalized(interSeconds, last.Elapsed, 0.5f);
							if (diff > 2)
								interSeconds = MathUtils.LerpNormalized(interSeconds, last.Elapsed, 0.25f);
							if (diff > 1)
								interSeconds = MathUtils.LerpNormalized(interSeconds, last.Elapsed, 0.175f);
							else if (diff > 0.5f)
								interSeconds = MathUtils.LerpNormalized(interSeconds, last.Elapsed, 0.1f);
							else if (!wereAhead)
							{
								if (diff > 0.1f)
								{
									interSeconds += 0.001f;
									factor       *= 1.1f;
								}

								if (diff > 0.01f)
								{
									interSeconds += 0.001f;
									factor       *= 0.999f;
								}

								if (diff < 0.01f)
									factor *= 0.99f;
							}
							else
							{
								factor *= 0.9f;
							}

							framesAdded    += (int) Math.Floor((diff / calcul.delta) * 0.5f);
							calcul.lastRaw =  TimeSpan.FromSeconds(last.Elapsed);

							calcul.frame             = last.Frame;
							calcul.interpolatedFrame = last.Frame + framesAdded;
						}

						interSeconds += worldTime.Delta.TotalSeconds * factor;

						calcul.currentInterpolated = TimeSpan.FromSeconds(interSeconds);
						calcul.diffInterpolated    = calcul.lastRaw - calcul.currentInterpolated;

						// TODO: Get Rtt
						calcul.currentExtrapolated = TimeSpan.FromSeconds(interSeconds) + TimeSpan.FromSeconds(calcul.delta * 2);

						gameWorld.GetComponentData<CalculatedNetTime>(gameEntity.Handle) = calcul;
					}
				}
			}
		}
	}
}
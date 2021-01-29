using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Collections.Pooled;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Game.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.GamePlay.Units;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay.Events;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.GamePlay.Damage
{
	[UpdateAfter(typeof(GenerateDamageRequestSystem))]
	public class ApplyKnockbackSystem : GameAppSystem, IGameEventPass
	{
		private UnitStatusEffectComponentProvider statusProvider;
		private UnitPhysicsSystem                 unitPhysicsSystem;

		public ApplyKnockbackSystem([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref statusProvider);
			DependencyResolver.Add(() => ref unitPhysicsSystem);
		}

		private EntityQuery evQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			evQuery = CreateEntityQuery(new[] {typeof(TargetDamageEvent)});
		}


		public void OnGameEventPass()
		{
			var damageAccessor = GetAccessor<TargetDamageEvent>();
			foreach (var handle in evQuery)
			{
				ref var damageEvent = ref damageAccessor[handle];
				if (damageEvent.Damage > 0)
					continue;

				var currentKnockbackPower = -1f;
				if (TryGetComponentBuffer<DamageFrameDataStatusEffect>(handle, out var statusBuffer))
				{
					foreach (var status in statusBuffer)
						if (status.Type == StatusEffect.KnockBack && status.Power > 0)
							currentKnockbackPower = status.Power;
				}

				if (currentKnockbackPower < 0)
					continue;

				var playState = GetComponentData<UnitPlayState>(damageEvent.Victim.Handle);
				if (statusProvider.HasStatus(damageEvent.Victim.Handle, StatusEffect.KnockBack))
				{
					ref readonly var state    = ref statusProvider.GetStatusState(damageEvent.Victim.Handle, StatusEffect.KnockBack);
					ref readonly var settings = ref statusProvider.GetStatusSettings(damageEvent.Victim.Handle, StatusEffect.KnockBack);
					if (state.CurrentResistance >= 0 || currentKnockbackPower < state.CurrentResistance)
						continue;

					var force = new Vector3(-GetComponentData<UnitDirection>(damageEvent.Victim.Handle).Value, 0.8f, 0);
					// TODO: Force direction from component
					var unlerp = Math.Min(MathUtils.Unlerp(0, settings.Resistance, -state.CurrentResistance), 2);
					if (settings.Resistance <= 0.001f)
						unlerp = 1;
					
					force *= unlerp * MathUtils.RcpSafe(playState.Weight * 0.25f);
					if (TryGetComponentData(handle, out DamageFrameData frameData))
						force *= frameData.KnockBackPower;

					//Console.WriteLine($"{state.CurrentResistance} {settings.Resistance} ({Math.Min(MathUtils.Unlerp(0, settings.Resistance, -state.CurrentResistance), 2)}) {playState.Weight} ({MathUtils.RcpSafe(playState.Weight * 0.25f)}) {frameData.KnockBackPower}");

					unitPhysicsSystem.Scheduler.Schedule(args =>
					{
						GetComponentData<Velocity>(args.Victim).Value += force;
					}, (force, damageEvent.Victim), default);
				}
			}
		}
	}
}
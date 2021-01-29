using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Passes;
using PataNext.Module.Simulation.Systems;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.GamePlay.Abilities
{
	public class ApplyAbilityStatisticOnChainingSystem : GameAppSystem, IAbilityPreSimulationPass
	{
		private PooledList<ComponentReference> stateRefs;

		public ApplyAbilityStatisticOnChainingSystem(WorldCollection collection) : base(collection)
		{
			AddDisposable(stateRefs = new PooledList<ComponentReference>());
		}

		private EntityQuery abilityQuery;

		public void OnAbilityPreSimulationPass()
		{
			var chargeComponentType = AsComponentType<ChargeCommand>();

			var ownerAccessor     = new ComponentDataAccessor<Owner>(GameWorld);
			var stateAccessor     = new ComponentDataAccessor<AbilityState>(GameWorld);
			var modifyAccessor    = new ComponentDataAccessor<AbilityModifyStatsOnChaining>(GameWorld);
			var engineSetAccessor = new ComponentDataAccessor<AbilityEngineSet>(GameWorld);
			var playStateAccessor = new ComponentDataAccessor<UnitPlayState>(GameWorld);
			foreach (var entity in (abilityQuery ??= CreateEntityQuery(new[]
			{
				AsComponentType<AbilityModifyStatsOnChaining>(),
				AsComponentType<AbilityEngineSet>(),
				AsComponentType<Owner>(),
			})))
			{
				ref readonly var state = ref stateAccessor[entity];
				if ((state.Phase & EAbilityPhase.ActiveOrChaining) == 0)
					continue;

				var              owner     = ownerAccessor[entity].Target;
				ref readonly var modify    = ref modifyAccessor[entity];
				ref readonly var engineSet = ref engineSetAccessor[entity];

				ref var playState  = ref playStateAccessor[owner.Handle];
				var     hasCharged = HasComponent(engineSet.PreviousCommand.Handle, chargeComponentType);

				stateRefs.Clear();
				GameWorld.GetComponentOf(owner.Handle, AsComponentType<StatusEffectStateBase>(), stateRefs);

				if (hasCharged && modify.SetChargeModifierAsFirst)
					modify.ChargeModifier.Multiply(ref playState, stateRefs, GameWorld);

				playState = AbilityUtility.CompileStat(engineSet, playState, modify.ActiveModifier, modify.FeverModifier, modify.PerfectModifier, stateRefs, GameWorld);

				if (hasCharged && !modify.SetChargeModifierAsFirst)
					modify.ChargeModifier.Multiply(ref playState, stateRefs, GameWorld);
			}
		}
	}
}
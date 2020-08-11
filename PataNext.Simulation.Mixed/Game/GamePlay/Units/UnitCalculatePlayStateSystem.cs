using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.GamePlay.Units
{
	public class UnitCalculatePlayStateSystem : GameAppSystem, IUpdateSimulationPass
	{
		public UnitCalculatePlayStateSystem(WorldCollection collection) : base(collection)
		{
		}

		private EntityQuery unitQuery;

		public void OnSimulationUpdate()
		{
			var unitStatisticsAccessor = new ComponentDataAccessor<UnitStatistics>(GameWorld);
			var unitPlayStateAccessor  = new ComponentDataAccessor<UnitPlayState>(GameWorld);
			var comboStateAccessor     = new ComponentDataAccessor<GameCombo.State>(GameWorld);
			var comboSettingsAccessor  = new ComponentDataAccessor<GameCombo.Settings>(GameWorld);
			foreach (var entity in (unitQuery ??= CreateEntityQuery(new[]
			{
				GameWorld.AsComponentType<UnitPlayState>(),
				GameWorld.AsComponentType<UnitStatistics>(),
			})).GetEntities())
			{
				ref readonly var original = ref unitStatisticsAccessor[entity];
				ref var          state    = ref unitPlayStateAccessor[entity];

				GameCombo.State    comboState    = default;
				GameCombo.Settings comboSettings = default;
				if (TryGetComponentData(entity, out Relative<RhythmEngineDescription> engineRelative))
				{
					comboState    = comboStateAccessor[engineRelative.Target];
					comboSettings = comboSettingsAccessor[engineRelative.Target];
				}

				state.MovementSpeed       = comboSettings.CanEnterFever(comboState) ? original.FeverWalkSpeed : original.BaseWalkSpeed;
				state.Defense             = original.Defense;
				state.Attack              = original.Attack;
				state.AttackSpeed         = original.AttackSpeed;
				state.AttackSeekRange     = original.AttackSeekRange;
				state.MovementAttackSpeed = original.MovementAttackSpeed;
				state.MovementReturnSpeed = comboSettings.CanEnterFever(comboState) ? original.MovementAttackSpeed * 1.7f : original.MovementAttackSpeed;
				state.Weight              = original.Weight;

				state.ReceiveDamagePercentage = 1;
			}
		}
	}
}
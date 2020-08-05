using GameHost.Core.Ecs;
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
			foreach (var entity in (unitQuery ??= CreateEntityQuery(new[]
			{
				GameWorld.AsComponentType<UnitPlayState>(),
				GameWorld.AsComponentType<UnitStatistics>(),
			})).GetEntities())
			{
				ref readonly var original = ref GetComponentData<UnitStatistics>(entity);
				ref var          state    = ref GetComponentData<UnitPlayState>(entity);

				GameCombo.State    comboState    = default;
				GameCombo.Settings comboSettings = default;
				if (TryGetComponentData(entity, out Relative<RhythmEngineDescription> engineRelative))
				{
					comboState    = GetComponentData<GameCombo.State>(engineRelative.Target);
					comboSettings = GetComponentData<GameCombo.Settings>(engineRelative.Target);
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
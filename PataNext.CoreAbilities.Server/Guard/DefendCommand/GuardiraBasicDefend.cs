using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.CoreAbilities.Mixed.CGuard;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;

namespace PataNext.CoreAbilities.Server.Guard.DefendCommand
{
	public class GuardiraBasicDefend : ScriptBase<GuardiraBasicDefendAbilityProvider>
	{
		public GuardiraBasicDefend(WorldCollection collection) : base(collection)
		{
		}

		protected override void OnSetup(Span<GameEntityHandle> abilities)
		{
		}

		protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
		{
			if (!state.IsActiveOrChaining || (GetComponentDataOrDefault(owner, new GroundState {Value = true}).Value == false))
				return;

			ref var control = ref GetComponentData<AbilityControlVelocity>(self);
			if (!state.IsActive)
			{
				if (state.IsChaining)
				{
					control.StayAtCurrentPositionX(5);
				}

				return;
			}
			
			control.ResetPositionX(50);
		}
	}
}
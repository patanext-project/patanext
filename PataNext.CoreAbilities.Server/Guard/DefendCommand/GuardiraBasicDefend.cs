using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.CoreAbilities.Mixed.CGuard;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;

namespace PataNext.CoreAbilities.Server.Guard.DefendCommand
{
	public class GuardiraBasicDefend : ScriptBase<GuardiraBasicDefendAbilityProvider>
	{
		public GuardiraBasicDefend(WorldCollection collection) : base(collection)
		{
		}

		protected override void OnSetup(GameEntity   self)
		{
		}

		protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
		{
			if (!state.IsActiveOrChaining)
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
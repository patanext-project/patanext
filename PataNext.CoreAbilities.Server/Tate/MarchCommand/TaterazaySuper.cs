using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.CoreAbilities.Mixed.CTate;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase.Physics.Components;

namespace PataNext.CoreAbilities.Server.Tate.MarchCommand
{
	public class TaterazaySuper : ScriptBase<TaterazaySuperAbilityProvider>
	{
		public TaterazaySuper(WorldCollection collection) : base(collection)
		{
		}

		protected override void OnSetup(Span<GameEntityHandle> abilities)
		{
		}

		protected override void OnExecute(GameEntity           owner, GameEntity self, ref AbilityState state)
		{
			ref var abilityState = ref GetComponentData<TaterazaySuperAbility.State>(self);
			
			var settings = GetComponentData<TaterazaySuperAbility>(self);
			if (state.IsActive)
			{
				var previousCommand = GetComponentData<AbilityEngineSet>(self).PreviousCommand.Entity;
				if (HasComponent<ChargeCommand>(previousCommand))
				{
					if (abilityState.LastUpdateActivation != state.ActivationVersion)
						GetComponentData<Velocity>(owner).Value.Y = settings.JumpPower;
				}
				else
				{
					GetComponentData<Velocity>(owner).Value.X                         = settings.Speed;
					GetComponentData<UnitControllerState>(owner).ControlOverVelocityX = true;
				}
			}

			abilityState.LastUpdateActivation = state.ActivationVersion;
		}
	}
}
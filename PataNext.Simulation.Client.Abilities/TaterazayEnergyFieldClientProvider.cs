using GameHost.Core.Ecs;
using PataNext.Game.Abilities;
using PataNext.Simulation.Mixed.Abilities.CTate;

namespace PataNext.Simulation.Client.Abilities
{
	public class TaterazayEnergyFieldClientProvider : BaseClientAbilityProvider<TaterazayEnergyFieldAbilityProvider>
	{
		public TaterazayEnergyFieldClientProvider(WorldCollection collection) : base(collection)
		{
		}
	}
}
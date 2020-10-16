using GameHost.Core.Ecs;
using PataNext.CoreAbilities.Mixed.CTate;
using PataNext.Game.Abilities;

namespace PataNext.Simulation.Client.Abilities
{
	public class TaterazayEnergyFieldClientProvider : BaseClientAbilityProvider<TaterazayEnergyFieldAbilityProvider>
	{
		public TaterazayEnergyFieldClientProvider(WorldCollection collection) : base(collection)
		{
		}
	}
}
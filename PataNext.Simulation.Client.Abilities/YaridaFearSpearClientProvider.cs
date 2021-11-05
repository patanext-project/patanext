using GameHost.Core.Ecs;
using PataNext.CoreAbilities.Mixed.CTate;
using PataNext.CoreAbilities.Mixed.CYari;
using PataNext.Game.Abilities;

namespace PataNext.Simulation.Client.Abilities
{
	public class YaridaFearSpearClientProvider : BaseClientAbilityProvider<YaridaFearSpearAbilityProvider>
	{
		public YaridaFearSpearClientProvider(WorldCollection collection) : base(collection)
		{
		}
	}
}
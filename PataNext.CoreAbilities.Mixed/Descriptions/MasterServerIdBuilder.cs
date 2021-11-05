using PataNext.Module.Simulation.BaseSystems;
using StormiumTeam.GameBase;

namespace PataNext.CoreAbilities.Mixed.Descriptions
{
	public static class MasterServerIdBuilder
	{
		public static string MegaponAbility => "mega";
		
		public static string GetAbility(this ResPathGen resPath, string kit, string abilityId)
		{
			return resPath.Create(new[] {"ability", kit, abilityId}, ResPath.EType.MasterServer);
		}
	}
}
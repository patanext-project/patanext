using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;

namespace PataNext.CoreAbilities.Mixed.CGuard
{
	public struct GuardiraBasicDefendAbility : IComponentData
	{
		
	}
	
	public class GuardiraBasicDefendAbilityProvider : BaseRuntimeRhythmAbilityProvider<GuardiraBasicDefendAbility>
	{
		protected override string FilePathPrefix => "guard";

		public GuardiraBasicDefendAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		public override string MasterServerId => resPath.Create(new [] {"ability", "guard", "def_def"}, ResPath.EType.MasterServer);

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<DefendCommand>();
		}
	}
}
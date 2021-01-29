using GameHost.Core.Ecs;
using JetBrains.Annotations;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Interfaces;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Components.Army
{
	public struct ArmyFormationDescription : IEntityDescription
	{
		public class RegisterRelative : Relative<ArmyFormationDescription>.Register
		{
		}

		public class RegisterContainer : BuildContainerSystem<ArmyFormationDescription>
		{
			public RegisterContainer([NotNull] WorldCollection collection) : base(collection)
			{
			}
		}
	}
	
	public struct ArmySquadDescription : IEntityDescription
	{
		public class RegisterRelative : Relative<ArmySquadDescription>.Register
		{
		}

		public class RegisterContainer : BuildContainerSystem<ArmySquadDescription>
		{
			public RegisterContainer([NotNull] WorldCollection collection) : base(collection)
			{
			}
		}
	}
	
	public struct ArmyUnitDescription : IEntityDescription
	{
		public class RegisterRelative : Relative<ArmyUnitDescription>.Register
		{
		}

		public class RegisterContainer : BuildContainerSystem<ArmyUnitDescription>
		{
			public RegisterContainer([NotNull] WorldCollection collection) : base(collection)
			{
			}
		}
	}
}
using GameHost.Core.Ecs;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Interfaces;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.Roles.Descriptions
{
	/// <summary>
	/// Represent an entity that is a health module for an entity.
	/// </summary>
	public struct HealthDescription : IEntityDescription
	{
		public class RegisterRelative : Relative<HealthDescription>.Register
		{
		}
		
		public class RegisterContainer : BuildContainerSystem<HealthDescription>
		{
			public RegisterContainer(WorldCollection collection) : base(collection)
			{
			}
		}
		
		public class Register : RegisterGameHostComponentData<HealthDescription>
		{
		}
	}
}
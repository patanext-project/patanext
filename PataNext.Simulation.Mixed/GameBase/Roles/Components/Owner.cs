using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace GameBase.Roles.Components
{
	public readonly struct Owner : IComponentData
	{
		public readonly GameEntity Target;

		public Owner(GameEntity target)
		{
			Target = target;
		}

		public class Register : RegisterGameHostComponentData<Owner>
		{
		}
	}
}
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace GameBase.Roles.Interfaces
{
	/// <summary>
	/// An entity description is a role attributed to a <see cref="GameEntity"/>
	/// </summary>
	public interface IEntityDescription : IComponentData
	{
	}
}
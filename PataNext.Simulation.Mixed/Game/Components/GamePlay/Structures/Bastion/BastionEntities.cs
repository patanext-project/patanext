using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Structures.Bastion
{
	public struct BastionEntities : IComponentBuffer
	{
		/// <summary>
		/// The entity should be linked to the bastion, can be null or non-existing
		/// </summary>
		public GameEntity Entity;

		public BastionEntities(GameEntity entity)
		{
			Entity = entity;
		}
	}
}
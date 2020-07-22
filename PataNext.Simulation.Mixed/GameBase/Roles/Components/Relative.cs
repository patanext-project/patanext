using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace GameBase.Roles.Components
{
	/// <summary>
	/// A relative path to a <see cref="GameEntity"/> with <see cref="Interfaces.IEntityDescription"/>
	/// </summary>
	/// <typeparam name="TDescription"></typeparam>
	public readonly struct Relative<TDescription> : IComponentData
		where TDescription : Interfaces.IEntityDescription
	{
		/// <summary>
		/// Path to the entity
		/// </summary>
		public readonly GameEntity Target;

		public Relative(GameEntity target)
		{
			Target = target;
		}
		
		public abstract class Register : RegisterGameHostComponentSystemBase<Relative<TDescription>>
		{
		}
	}
}
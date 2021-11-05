using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Components.GamePlay;

namespace PataNext.Module.Simulation.Game.Visuals
{
	/// <summary>
	/// Represent an entity with a graphical resource that can be shown on the client.
	/// </summary>
	public readonly struct EntityVisual : IComponentData
	{
		// If default, the client should try to know which resource to use.
		//
		// It should be based on the current animation to determinate it.
		// If it's Yarida attack animation, it should be its current spear.
		// But if it's Taterazay doing a slash animation, then we don't know and it will be invisible.
		//
		// If the projectile graphic matter and shouldn't be based on the animation, set resource.
		// It should only be set to default if the client can guess it and would do a better job than us at choosing the graphic.
		//
		// If you set it to non-default value, but still want the client to be able to guess it if possible, then set 'ClientPriority' to 'True'.
		public readonly GameResource<GameGraphicResource> Resource;

		/// <summary>
		/// If true, the client can choose to change graphic projectile.
		/// </summary>
		/// <remarks>
		/// If resource is default and this variable is 'False', client will assume that it can select the graphic.
		/// </remarks>
		public readonly bool ClientPriority;

		public EntityVisual(GameResource<GameGraphicResource> resource, bool clientPriority = false)
		{
			Resource       = resource;
			ClientPriority = clientPriority;
		}

		public class Register : RegisterGameHostComponentData<EntityVisual>
		{

		}
	}
}
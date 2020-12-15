using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs.Interfaces;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.Network
{
	public class SetRemoteAuthoritySystem : GameAppSystem
	{
		public SetRemoteAuthoritySystem(WorldCollection collection) : base(collection)
		{
		}
	}

	/// <summary>
	/// Set the remote authority. If there is no remote, we add the authority to ourselves.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public struct SetRemoteAuthority<T> : IComponentData
	{

	}
}
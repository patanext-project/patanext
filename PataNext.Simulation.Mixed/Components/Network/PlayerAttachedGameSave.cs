using GameHost.Native.Char;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.Network
{
	public struct PlayerAttachedGameSave : IComponentData
	{
		public CharBuffer64 Guid;

		public PlayerAttachedGameSave(CharBuffer64 guid) => Guid = guid;
	}
}
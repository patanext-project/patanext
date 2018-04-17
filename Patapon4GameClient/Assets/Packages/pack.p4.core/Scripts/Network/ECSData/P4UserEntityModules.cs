using System.Runtime.InteropServices;
using Packet.Guerro.Shared.Network;
using Unity.Entities;

namespace P4.Core.Network
{
	/// <summary>
	/// An extension to <see cref="UserEntity"/> with masterserver informations.
	/// </summary>
	public struct UserAccountEntity : IUserEntityModule
	{
		public int ReferenceId;
		public int MasterId;
	}

	/// <summary>
	/// An extension to <see cref="UserEntity"/> with gameserver informations.
	/// </summary>
	public struct UserGameEntity : IUserEntityModule
	{
		public int ReferenceId;
		public int GameId;
	}

	public static class P4UserEntityModulesExtensions
	{
		public static CDataUser GetDataUser(this UserAccountEntity component)
		{
			return CDataUser.Get(component.ReferenceId);
		}
		public static CDataUser GetDataUser(this UserGameEntity component)
		{
			return CDataUser.Get(component.ReferenceId);
		}
	}
}
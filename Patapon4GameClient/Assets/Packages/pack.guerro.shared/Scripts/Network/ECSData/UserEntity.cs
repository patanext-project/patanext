using System.Runtime.InteropServices;
using Packet.Guerro.Shared;
using Unity.Entities;

namespace Packet.Guerro.Shared.Network
{
    public interface IUserEntityModule : IComponentData
    {
        
    }

    /// <summary>
    /// An user
    /// </summary>
    public struct UserEntity : IComponentData
    {
        /// <summary>
        /// Used to track the User.
        /// </summary>
        public int ReferenceId;
    }

    public static class UserEntityExtensions
    {
        public static CDataUser GetDataUser(this UserEntity component)
        {
            return CDataUser.Get(component.ReferenceId);
        }
    }
}
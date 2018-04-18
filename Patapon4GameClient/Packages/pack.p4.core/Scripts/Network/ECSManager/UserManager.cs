using System.Collections.Generic;
using Packet.Guerro.Shared.Network;
using Unity.Entities;

namespace P4.Core.Network
{
    public class UserManager : ComponentSystem
    {
        private List<CDataUser> m_Users;

        public CDataUser CompleteCreate(CDataUser userInfo)
        {
            var user = CDataUser.Create(userInfo);
            var entity = user.ToEntity(World);
            
            CDataUser.Update(user);

            return user;
        }

        /*public CDataUser StrippedCreate(CDataUser userInfo)
        {
            
        }*/

        protected override void OnUpdate()
        {
        }

        protected override void OnDestroyManager()
        {
            m_Users?.Clear();
            m_Users = null;
        }
    }
}
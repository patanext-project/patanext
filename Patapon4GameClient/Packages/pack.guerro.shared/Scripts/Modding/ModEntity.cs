using System;
using Packet.Guerro.Shared;
using Packet.Guerro.Shared.Clients;
using Unity.Entities;

namespace Packages.pack.guerro.shared.Scripts.Modding
{
    /*
     * Yeah, mods got an entity. Any problem with that?
     * Yeah, maybe no
     */
    /*
    public struct ModEntity : IComponentData, IEquatable<ModEntity>
    {
        public int  ReferenceId;
        public bool IsCreated;

        public bool Equals(ModEntity other)
        {
            return ReferenceId == other.ReferenceId && IsCreated == other.IsCreated;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ModEntity && Equals((ModEntity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                var hashCode = ReferenceId;
                hashCode = (hashCode * 397) ^ IsCreated.GetHashCode();
                return hashCode;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }
    }
    
    public static class ModEntityExtensions
    {
        public static bool IsAlive(this ModEntity modEntity, World world = null)
        {
            if (world == null)
                world = World.Active;

            return world.GetExistingManager<CModManager>().Exists(modEntity);
        }

        public static CModInfo GetInfo(this ModEntity modEntity, World world = null)
        {
            if (world == null)
                world = World.Active;

            return world.GetExistingManager<CModManager>().GetInfoFromId(modEntity);
        }
    }*/
}
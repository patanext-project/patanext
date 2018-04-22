using System;
using Packages.pack.guerro.shared.Scripts.Clients;
using Packet.Guerro.Shared.Inputs;
using Unity.Entities;
using Unity.Mathematics;

namespace Packet.Guerro.Shared.Clients
{
    public struct ClientEntity : IComponentData, IEquatable<ClientEntity>
    {
        public int ReferenceId;
        public int Version;
        public bool1 IsCreated;

        public bool Equals(ClientEntity other)
        {
            return ReferenceId == other.ReferenceId && Version == other.Version && IsCreated == other.IsCreated;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ClientEntity && Equals((ClientEntity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                var hashCode = ReferenceId;
                hashCode = (hashCode * 397) ^ Version;
                hashCode = (hashCode * 397) ^ IsCreated.GetHashCode();
                return hashCode;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }
    }

    public static class ClientEntityExtensions
    {
        public static bool IsAlive(this ClientEntity clientEntity, World world = null)
        {
            if (world == null)
                world = World.Active;

            return world.GetExistingManager<ClientManager>().Exists(clientEntity);
        }
        
        public static ClientWorld GetWorld(this ClientEntity clientEntity, World world = null)
        {
            if (world == null)
                world = World.Active;

            return world.GetExistingManager<ClientManager>().GetWorld(clientEntity);
        }
        
        public static ClientInputManager GetInputManager(this ClientEntity clientEntity)
        {
            return clientEntity.GetWorld().GetOrCreateManager<ClientInputManager>();
        }
    }
}
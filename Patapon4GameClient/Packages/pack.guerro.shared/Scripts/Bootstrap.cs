using Packages.pack.guerro.shared.Scripts.Modding;
using Packet.Guerro.Shared.Clients;
using Packet.Guerro.Shared.Inputs;
using Unity.Entities;

namespace Packet.Guerro.Shared
{
    public class Bootstrap : CModBootstrap
    {
        protected override void OnRegister()
        {
            // Register events
            ClientManager.OnNewClient += OnNewClient;
            

        }

        protected override void OnUnregister()
        {
            
        }

        private void OnNewClient(ClientEntity clientEntity)
        {
            var world = clientEntity.GetWorld();
            
            // Create managers
            world.GetOrCreateManager<ClientInputManager>();
        }
    }
}
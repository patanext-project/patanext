using System.Reflection;
using Unity.Entities;
using UnityEngine;
using P4.Core.Network;
using Packages.pack.guerro.shared.Scripts.Modding;
using Packet.Guerro.Shared;
using Packet.Guerro.Shared.Clients;
using Packet.Guerro.Shared.ECS;
using Packet.Guerro.Shared.Network;

namespace P4Main
{
    public class GameBoostrap
    {
        /*
         * Logically, the automatic world creation should be done before this.
         */
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            CWorldBootstrap.Init();
            
            var register = CModManager.BeginInternalRegistration();
            {
                register.AddInternalPacket
                (
                    "Patapon Core",
                    "pack.p4.core",
                    new[]
                    {
                        Assembly.GetAssembly(typeof(P4.Core.Bootstrap))
                    }
                );
                register.AddInternalPacket
                (
                    "Patapon Default Assets",
                    "pack.p4.default",
                    new[]
                    {
                        Assembly.GetAssembly(typeof(P4.Default.Bootstrap))
                    }
                );
            }
            register.End();
            
            //Eudi.Globals.SetBindingFromInstance<UserManager>(new UserManager());

            ClientManager.EnableMultiClient = false;
            
            // Register default client, and because we hate MultiClient, we aren't going to use it.
            var clientManager = World.Active.GetOrCreateManager<ClientManager>();
            clientManager.Create("MainUser");

            // Lol
            var currentWorld = World.Active;

            // We have a way to create user to test things (so, not a real user.)
            var testUser = CDataUser.Create();
            testUser.Login = "An interesting user";
            testUser.ToEntity(currentWorld);
            CDataUser.Update(testUser);

            // And we have another way, the complete way (so, a real user)
            var userManager = currentWorld.GetOrCreateManager<UserManager>();
            var realUser = userManager.CompleteCreate(new CDataUser() 
            { 
                Login = "A real user" 
            });

            var manager = currentWorld.GetExistingManager<EntityManager>();
            foreach (var entity in manager.GetAllEntities())
            {
                if (manager.HasComponent<UserEntity>(entity))
                {
                    Debug.Log("Found back entity login: " + manager.GetComponentData<UserEntity>(entity)
                    .GetDataUser()
                    .Login);
                }
            }
        }
    }
}
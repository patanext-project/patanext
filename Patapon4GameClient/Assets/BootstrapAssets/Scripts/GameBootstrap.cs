using Unity.Entities;
using UnityEngine;
using P4.Core.Network;
using Packet.Guerro.Shared.Network;

namespace P4Main
{
    public class GameBoostrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Init()
        {
            //Eudi.Globals.SetBindingFromInstance<UserManager>(new UserManager());

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
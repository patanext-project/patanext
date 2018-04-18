using System.Collections;
using System.Collections.Generic;
using P4.Default.Movements;
using Packages.pack.guerro.shared.Scripts.Utilities;
using Packet.Guerro.Shared.Game;
using Packet.Guerro.Shared.Network;
using Packet.Guerro.Shared.Network.Entities;
using Unity.Entities;
using UnityEngine;

public class UnitFreeHeroMovement_Bootstrap : MonoBehaviour
{
    public GameObject PrefabCharacter;

    private void Awake()
    {
        AddNewCharacter();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var go = AddNewCharacter();
            go.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    public GameObject AddNewCharacter()
    {
        var characterGo = Instantiate(PrefabCharacter);
        characterGo.SetActive(true);
        
        var characterEntity           = characterGo.GetComponent<GameObjectEntity>().Entity;
        var entityManager             = World.Active.GetExistingManager<EntityManager>();
        var networkEntityManager      = World.Active.GetExistingManager<CNetworkEntityManager>();
        var controllableEntityManager = World.Active.GetExistingManager<CGameControllableEntityManager>();

        var netData = new NetworkEntity
        {
            IsLocal          = true,
            LocalControlId   = 0,
            NetworkControlId = -1
        };
        var controlData = new ControllableEntity
        {
            ControlType = EEntityControl.Always
        };

        networkEntityManager.AddOrSetComponent(characterEntity, characterGo, netData);
        controllableEntityManager.AddOrSetComponent(characterEntity, characterGo, controlData);

        characterGo.AddComponentToEntity<P4Default_DFreeMovementWrapper>();
        return characterGo;
    }
}

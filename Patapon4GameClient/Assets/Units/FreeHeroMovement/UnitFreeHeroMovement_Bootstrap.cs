using System;
using System.Collections;
using System.Collections.Generic;
using P4.Core.Graphics;
using P4.Default;
using P4.Default.Inputs;
using P4.Default.Movements;
using Packages.pack.guerro.shared.Scripts.Utilities;
using Packet.Guerro.Shared.Game;
using Packet.Guerro.Shared.Network;
using Packet.Guerro.Shared.Network.Entities;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Playables;

public class UnitFreeHeroMovement_Bootstrap : MonoBehaviour
{
    public GameObject PrefabCharacter;
    public bool EnableStresstest;

    private int m_Count;
    private float m_Delay;

    public InputAction WalkAction;
    
    private void Awake()
    {
        foreach (var inputDevice in InputSystem.devices)
        {
            Debug.Log($"device:{inputDevice.id}\nuser:{inputDevice.userId}\nname:{inputDevice.description.deviceClass}");
        }

        AddNewCharacter();
    }

    private void Update()
    {
        if (EnableStresstest)
        {
            if ((Input.GetMouseButton(0) && m_Delay <= 0f)
                || Input.GetMouseButtonDown(0))
            {
                m_Delay = 0.01f;

                var go       = AddNewCharacter();
                var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;

                go.transform.position = mousePos;
            }

            m_Delay -= Time.deltaTime;
        }

        if (Input.mouseScrollDelta.y != 0f)
        {
            Camera.main.orthographicSize += Input.mouseScrollDelta.y * 12.5f * Time.deltaTime;
        }
    }

    private void OnGUI()
    {
        GUI.color = Color.black;
        GUI.Label(new Rect(5, 5, 300, 120), $"(Stress test) Characters: {m_Count}");
        GUI.Label(new Rect(25, 25, 300, 120), $"Ignored splines: {World.Active.GetExistingManager<SplineSystem>().IgnoredSplines}");
    }

    public GameObject AddNewCharacter()
    {
        m_Count++;
        
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
        characterGo.AddComponentToEntity<P4Default_DEntityInputFreeWrapper>();

        var cps = entityManager.GetComponentTypes(characterEntity);
        foreach (var component in cps)
        {
            Debug.Log(component.GetManagedType());
        }
        return characterGo;
    }
}

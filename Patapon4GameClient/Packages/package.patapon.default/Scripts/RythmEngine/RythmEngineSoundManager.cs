using System.Collections;
using System.Collections.Generic;
using Packages.pack.guerro.shared.Scripts.Clients;
using Packet.Guerro.Shared.Clients;
using Unity.Entities;
using UnityEngine;

public class RythmEngineSoundManager : ComponentSystem
{
    [Inject] private SharedClientGroup m_ClientGroup;
    
    protected override void OnUpdate()
    {
        // We only implement it for one client right now
        for (int i = 0; i != m_ClientGroup.Length; i++)
        {
            if (i != 0)
                return;

            ForeachClientUpdate(m_ClientGroup.Clients[i]);
        }
    }

    private void ForeachClientUpdate(ClientEntity clientEntity)
    {
        
    }
}

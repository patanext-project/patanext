This page isn't finished

```c#
// Client, ryth movement
class InputSystem : System
{
    struct Group
    {
        public DataArray<ClientEntity> Clients;
    }

    override void OnUpdate()
    {
        // basic iterator...
        for (int i = 0; i != m_Group.Length; i++)
        {
            var clientId = m_Group.Clients[i].ReferenceId;
            <...>
        }

        // Classic iterator for non systems classes.
        for (int i = 0; i != CGameClient.Count; i++)
        {
            // Get the ID of the client (we don't store data in the clients.)
            var clientId = CGameClient.GetId(i);
            // Get the input device from the client
            var device = CInput.GetDeviceFromClientId(clientId);
            // Get the user from the client
            var user = CGameClient.GetUserFromId(clientId);

            var userEntity = user.ToEntityWorld();
            var inputComponent = userEntity.GetComponentData<DFooInputData>();

            inputComponent.LeftKey = device.Get();

            userEntity.SetComponentData(inputComponent);
        }       
    }
}
```

```c#

```

CGameClient.DisconnectId(clientId);
var id = CGameClient.Connect(..., ...);
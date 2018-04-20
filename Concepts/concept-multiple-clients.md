<html>
    <p align="center">
    <img src="https://pre00.deviantart.net/4960/th/pre/i/2017/334/1/6/_patapon_4_tlb__p4_logo_variant_2_by_guerro323-dbvceq0.png" alt="Super Logo!" width="64" height="64" />
    </p>
    <h2 align="center">
    Patapon 4 - The Last Battle - Concept: Multiple clients on the same game
    </h2>
</html>

The game client will have the possibility of having multiple clients on the same instance, thanks to ECS paradigm.

### Positives points:
- Splitscreen, because why not? (The only problem for me would be how I could send the right sounds states...)
- Even if I don't do that, some people will just open a new instance of the game to connect with another client.

### Negative points
- People can farm easily with 2, 3, 4 accounts.
- It's complicate to understand the concept of multiple clients for mods

The client itself don't hold the data, it's just an identificator, we don't use an Entity for the ID, as modders will maybe use the first ID (0) to get the client.
There will be a maximum of 4 clients per instances. (But I recommend for your mods to have a support for more clients, we never know!)

___

## Code
Go to:
- [Code examples](#Examples)

___

### Client code
```c#
struct ClientEntity : IComponentData
{
    int ReferenceId;
    bool IsCreated;

    // Is the client created and not in reset state? (It's a fast path to ClientManager.Exists(ReferenceId))
    extension bool IsAlive();
}

class ClientManager : Manager
{
    // Subclasses
    struct SearchFilter
    {
        int SearchMainClient;
        int SearchLivableClients;
    }

    // Fields
    int          Count; // From alive clients

    // Creation
    ClientEntity Connect      (string Login, string Password);
    ClientEntity Create       ();
    ClientEntity Create       (string UserLogin);
    // Retrieve
    ClientEntity Get          (int Index);
    bool         Exists       (ClientEntity Entity);
    bool         Exists       (int Id);
    // Destruction (reset)
    void         ResetByIndex (int Index);
    void         ResetById    (int Id);

    NativeArray<ClientEntity> GetClients(SearchFilter filter);

    // Foreach implementation?

    // The rest are extensions (GetUserFromId, ...)
}
```

References:
- [[SearchFilter variables explaination|Search-filters]]

___

Go to:
- [Client code](#client-code)

___

### Examples
First, you need to get the manager
```c#
var clientManager = World.Active.GetExistingManager<ClientManager>();
```

Create a client:
```c#
// By connection
var clientId = clientManager.Connect(login, password);
// Or if you don't want to connect (overload with user creation)
var clientId = clientManager.Create("customUser");
```

Retrieve a client:
```c#
// By index
var clientId = clientManager.Get(0);
```

Reset a client (you can't remove a client once it's created):
```c#
clientManager.ResetByIndex(0);
clientManager.ResetById(clientId);
```

Get the user from the client:
```c#
// From client Id
var user = clientManager.GetUserFromId(clientId);
// From client index
var user = clientManager.GetUserFromIndex(0);
```
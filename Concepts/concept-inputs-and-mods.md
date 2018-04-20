<html>
    <p align="center">
    <img src="https://pre00.deviantart.net/4960/th/pre/i/2017/334/1/6/_patapon_4_tlb__p4_logo_variant_2_by_guerro323-dbvceq0.png" alt="Super Logo!" width="64" height="64" />
    </p>
    <h2 align="center">
    Patapon 4 - The Last Battle - Concept: Inputs with mods
    </h2>
</html>

Directory structure:
```
/<Your Mod>
├── Inputs/
│   ├── <input file>.json
│   ├── <type>.<layout/device name>.<input file>.json
```

## Create our inputs

___

Fast go-to:
**Create our inputs:**
- [Input settings files creation](#files-creation)
- [Create input settings by scripting](#create-from-script)

*Register and access our inputs:*
- [Register our inputs](#script-creation)
- [Access to our inputs](#access-to-our-inputs)
___


### Files Creation

[If you prefer to create the default inputs from script.](#create-from-script)

Create an input file, let say you create default inputs for a Keyboard and a Gamepad layouts:   
**file: `Inputs/custom_inputs.json`**
```json
{
    "left_key":
    {
        "displayName": "Left Key",
        "translation": "%m.left_key"

        // You will also be able do this in future
        , "<keyboard>.default": "leftArrow"
    },
    "right_key":
    {
        "displayName": "right Key",
        "translation": "%m.right_key" 
    },    
}
```
**file: `Inputs/layout.keyboard.custom_inputs.json`**
```json
{
    "left_key":
    {
        "default": "leftArrow"
    },
    "right_key":
    {
        "default": "rightArrow"
    },    
}
```
**file: `Inputs/layout.gamepad.custom_inputs.json`**
```json
{
    "left_key":
    {
        "default": "dpad/left"
    },
    "right_key":
    {
        "default": "dpad/right"
    },    
}
```

### Create from script

You are also able to create the default inputs from script:
```c#
var inputList = new List<InputManager.InputInformation>();

myInputList.Add(new Input("left_key")
{
    DisplayName = "Left Key",
    Translation = "%m.left_key",
    Defaults = 
    {
        { "<keyboard>", ["leftArrow"] }
        { "<gamepad>", ["dpad/left"] }
    }
});
myInputList.Add(new Input("right_key")
{
    DisplayName = "Right Key",
    Translation = "%m.right_key",
    Defaults = 
    {
        { "<keyboard>", ["rightArrow"] }
        { "<gamepad>", ["dpad/right"] }
    }
});
```

## Register our inputs

___

Fast go-to:
*Create our inputs:*
- [Input settings files creation](#files-creation)
- [Create input settings by scripting](#create-from-script)

**Register and access our inputs:**
- [Register our inputs](#script-creation)
- [Access to our inputs](#access-to-our-inputs)
___


### Script creation

Now, let's create the script to play our new inputs, first, we need to create the structure to attach our new inputs.
In any classe that get created at the start (ex: bootstraps, managers), insert this:

```c#
public class MySetupClass
{
    [ModEvent.OnCreate]
    public void MyInit(CModInfo modInfo)
    {
        var inputManager = modInfo.InputManager;

        // If you did a file to register your default inputs
        inputManager.RegisterFromFile("custom_inputs");
        // If you've done a list to register your default inputs
        var inputList = new List<InputManager.InputInformation>();
        //...
        inputManager.RegisterFromList(myInputList);
    }
}
```

### Access to our inputs

```c#
public class MyInputSystem : System
{
    [ModEvent.InjectModInfo]
    ModInfo m_ModInfo;

    [Inject]
    InputManager m_GlobalInputManager;

    int m_LeftKeyId;
    int m_rightKeyId;

    public override void OnCreate()
    {
        //< -------- -------- -------- -------- -------- -------- -------- ------- //
        // Receive the input by events...
        //> -------- -------- -------- -------- -------- -------- -------- ------- //
        // Get the manager
        var inputManager = m_ModInfo.InputManager;

        // Get the ids of the inputs
        leftKeyId = inputManager.GetId("left_key");
        rightKeyId = inputManager.GetId("right_key");

        // Now, add our events (don't forget to remove them when you don't want to listen to them anymore!)
        m_GlobalInputManager.Get(leftKeyId)
            .OnEvent += (client, inputAction) => { Debug.Log($"ClientId {client.ReferenceId} has pressed the left key!") };
        m_GlobalInputManager.Get(rightKeyId)
            .OnEvent += (client, inputAction) => { Debug.Log($"ClientId {client.ReferenceId} has pressed the right key!") };
        // ...
    }

    [Inject] SharedClientGroup m_ClientGroup; //< A group constitued of ClientEntity data components

    public override void OnUpdate()
    {
        //< -------- -------- -------- -------- -------- -------- -------- ------- //
        // Receive the input with a game loop
        //> -------- -------- -------- -------- -------- -------- -------- ------- //
        for (int i = 0; i != m_ClientGroup.Length; i++)
        {
            // Get the input manager from the client (this is an extension method, as a client don't store any logic)
            var clientInputManager = m_clientGroups.Clients[i].GetClientInputManager();

            // Get our values...
            var leftKeyValue = clientInputManager.GetDirect<float>(leftKeyId).Value;
            var rightKeyValue = clientInputManager.GetDirect<float>(rightKeyId).Value;
        }
    }
}
```
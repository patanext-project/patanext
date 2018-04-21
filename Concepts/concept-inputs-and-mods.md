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

Create an input file, let say you create default inputs for a Keyboard and a Gamepad layouts, where you need to press Jump and move Left/Right:   
**file: `Inputs/custom_inputs.json`**
```json
{
    "jump":
    {
        "type": "push",

        "displayName": "Jump",
        "translation": "%m.inputs.jump"

        // You will also be able do this in future
        , "<keyboard>.default": "space"
    },
    "horizontal":
    {
        "type": "axis1d",

        "displayName": "Horizontal Movement",
        "translation": "%m.inputs.horizontal" 
    },    
}
```
**file: `Inputs/layout.keyboard.custom_inputs.json`**
```json
{
    "jump":
    {
        "default": "space"
    },
    "horizontal":
    {
        "-x": "leftArrow",
        "+x": "rightArrow"
    },    
}
```
**file: `Inputs/layout.gamepad.custom_inputs.json`**
```json
{
    "jump":
    {
        "default": "buttonSouth"
    },
    "horizontal":
    {
        "-x": "dpad/left",
        "+x": "dpad/right"
    },    
}
```

### Create from script

You are also able to create the default inputs from script:
```c#
var inputList = new Boo.Lang.List<CInputManager.IInputSetting>();

inputList.Add(new CInputManager.Settings.Push
    (
        nameId: "jump",
        displayName: "Jump",
        translation: "%m.inputs.jump",
        defaults: new FastDictionary<string, string[]>
        {
            {
                "<keyboard>",
                new [] { "space" }
            },
            {
                "<gamepad>",
                new [] { "buttonSouth" }
            }
        }
    )
);
inputList.Add(new CInputManager.Settings.Axis1D
    (
        nameId: "horizontal",
        displayName: "Horizontal",
        translation: "%m.inputs.horizontal",
        defaults: new FastDictionary<string, FastDictionary<string, string[]>>
        {
            {
                "<keyboard>",
                new FastDictionary<string, string[]>
                {
                    {"-x", new [] {"leftArrow"}},
                    {"+x", new [] {"rightArrow"}}
                }
            },
            {
                "<gamepad>",
                new FastDictionary<string, string[]>
                {
                    {"-x", new [] {"dpad/left"}},
                    {"+x", new [] {"dpad/right"}}
                }
            }
        }
    )
);
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
        var inputManager = modInfo.GetExistingManager<ModInputManager>();

        // If you did a file to register your default inputs
        inputManager.RegisterFromFile("custom_inputs");
        // If you've done a list to register your default inputs
        var inputList = new List<CInputManager.IInputSetting>();
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

    int m_jumpInputId;
    int m_horizontalInputId;

    public override void OnCreate()
    {
        //< -------- -------- -------- -------- -------- -------- -------- ------- //
        // Receive the input by events...
        //> -------- -------- -------- -------- -------- -------- -------- ------- //
        // Get the manager
        var inputManager = m_ModInfo.GetExistingManager<ModInputManager>();

        // Get the ids of the inputs
        m_jumpInputId = inputManager.GetId("jump");
        m_horizontalInputId = inputManager.GetId("m_horizontalInputId");

        // Now, add our events (don't forget to remove them when you don't want to listen to them anymore!)
        m_GlobalInputManager.Get<CInputManager.IInputEntry.Push>(m_jumpInputId)
            .OnEvent += (client, inputAction) => { Debug.Log($"ClientId {client.ReferenceId} has jumped!") };
        m_GlobalInputManager.Get<CInputManager.IInputEntry.Axis1D>(m_horizontalInputId)
            .OnEvent += (client, inputAction) => { Debug.Log($"ClientId {client.ReferenceId} is moving!") };
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
            var clientInputManager = m_clientGroups.Clients[i].GetExisting<ClientInputManager>();

            // Get our values...
            float jumpValue = clientInputManager.Get<CInputManager.Types.Push>(leftKeyId).Value;
            float horizontalValue = clientInputManager.Get<CInputManager.Types.Axis1D>(rightKeyId).Value;

            // Or get directly via the inputsystem
            var jumpControl = clientInputManager.GetFromDevice<InputControl<float>>(m_jumpInputId);
            // But as you can see, we can have some problem by getting the value directly by the inputsystem!
            var horizontalControl = clientInputManager.GetFromDevice<InputControl<Vector2>>(m_horizontalInputId);
        }
    }
}
```

# API:

```c#
public class InputManager
{
    public ModInfo AttachedModInfo { get; }

    // Register
    public void RegisterFromFile(string path);
    public void RegisterFromString(string @string);
    public void RegisterFromList(List<CInputManager.IInputSetting> listInputs);
    public void RegisterSingle(CInputManager.IInputSetting setting);

    // Get
    public void GetId(string inputMapName);
    public ??? Get<TInput>(int id);
    public CInputManager.Map GetMap(int|string id);

    public struct Map
    {
        public string NameId;
        public int Id;

        public IInputSetting UnknowSetting;
        public Settings.Push PushSetting;
        public Settings.Axis1D Axis1DSetting;

        public Settings.EType SettingType;
    }

    public interface IInputResult
    {
        object GetValueBoxed();
    }

    public interface IInputSetting
    {
        Type ResultType;
        string NameId;
        string DisplayName;
        string Translation;

        Dictionary<string, object> GetDefaults();
        bool ContainsLayout(string layoutType);
    }

    public static class Result
    {
        public struct Push : IInputResult {
            public float Value;
        }

        public struct Axis1D : IInputResult {
            public float Value;
        }

        public struct Axis2D : IInputResult {
            public Vector2 Value;
        }
    }

    public static class Setting
    {
        public struct Push : IInputSetting {
            
        }

        public struct Axis1D : IInputSetting {

        }

        public struct Axis2D : IInputSetting {

        }
    }
}
```
This page is more like a personnal page for me, and for now (because as you see, it's **very pseudo cody**).    
It's for the organization of the rythm engine.

Rythm engine will be located inside P4Default packet.
I don't want it to be located inside P4Core, as I want modders to create their own rythm engine if they want.

# Rythm inputs files
Folder location:
```
/<Your Mod>
├── RythmInputs/
│   ├── <rythm input file>.json // If we use json files
│   ├── <rythm input asset>.asset // If we use unity assets files
```

Example: `pon.json`
### Json structure file (will give use less possibility to custom it)?
```
{

}
```

### Unity assetbundle file?
Structure:
```

```

```c#
// Rythm Input processor
void OnInputStart(ctx, inputId)
{
    // Send inputs to rythm engine?
    RythmEngine.AddInputEvent(new InputEvent());
    // Or set the entity input value?
    foreach (entity in ClientEntities)
    {
        entity.InputData = inputData;
    }
    // OR BETTER (maybe), set a client container.
    client.SetContainer(rythmData);
}

// RythmEngineSystem
void OnUpdate()
{
    // First proposition
    foreach (event in PendingEvents)
    {
        ProcessTheEvent(event);

        removeEvent(event);
    }   
}
```
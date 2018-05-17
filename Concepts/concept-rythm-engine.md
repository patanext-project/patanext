This page is more like a personnal page for me, and for now (because as you see, it's **very pseudo cody**).    
It's for the organization of the rythm engine.

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
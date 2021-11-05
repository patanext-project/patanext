using Collections.Pooled;
using DefaultEcs;
using revghost.Ecs;
using revghost.Loop.EventSubscriber;
using revghost.Utility;

namespace PataNext.Export.Godot.Presentation;

public interface IPresentationLoop : IEventSubscriber
{
    Entity Subscribe(Action callback, ProcessOrder order = null);
}

public class PresentationLoop : IPresentationLoop,
    IDisposable
{
    private readonly OrderGroup _orderGroup;
    private Entity _callbackEntity;
    private readonly PooledList<Action> _callbacks = new(ClearMode.Always);

    public PresentationLoop(World world)
    {
        _orderGroup = new OrderGroup();
        _callbackEntity = world.CreateEntity();
    }

    public void Dispose()
    {
        _orderGroup.Dispose();
        _callbackEntity.Dispose();
    }

    public Entity Subscribe(Action<Entity> callback, ProcessOrder process)
    {
        return Subscribe(() => callback(_callbackEntity), process);
    }

    public Entity Subscribe(Action callback, ProcessOrder process)
    {
        var entity = _orderGroup.Add(process);
        entity.Set(in callback);
        return entity;
    }

    public void Invoke()
    {
        if (_orderGroup.Build())
        {
            _callbacks.ClearReference();
            var entities = _orderGroup.Entities;
            for (var index = 0; index < entities.Length; ++index)
                _callbacks.Add(entities[index].Get<Action>());
        }

        foreach (var action in _callbacks.Span)
            action();
    }
}
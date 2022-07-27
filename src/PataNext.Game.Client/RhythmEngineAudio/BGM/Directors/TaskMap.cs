using System.Runtime.CompilerServices;

namespace PataNext.Game.Client.RhythmEngineAudio.BGM.Directors;

public class TaskMap<TKey, TValue>
{
    public delegate Task<TValue> OnTask(TKey key);

    private readonly Dictionary<TKey, TValue> cached;

    private readonly Dictionary<TKey, Task<TValue>> map;

    private readonly OnTask onTask;

    public TaskMap(OnTask onTask)
    {
        this.onTask = onTask;
        map = new Dictionary<TKey, Task<TValue>>();
        cached = new Dictionary<TKey, TValue>();
    }

    public bool GetValue(TKey key, out TValue value, out Task<TValue> task)
    {
        if (cached.TryGetValue(key, out value))
        {
            task = map[key];
            return true;
        }

        if (map.TryGetValue(key, out task))
        {
            if (task.IsCompletedSuccessfully)
            {
                cached[key] = value = task.Result;
                return true;
            }

            value = default;
            return false;
        }

        AddTask(key);
        value = default;
        return false;
    }
    
    // this .NET version seems to allocate the lambda even if this section never get executed
    // so scope it into a method so that the JIT or the compiler or whatever will not do that.
    // TODO: check it in later versions if this get fixed
    [MethodImpl(MethodImplOptions.NoOptimization)]
    private void AddTask(TKey key)
    {
        map[key] = Task.Run(() => onTask(key));
    }
}
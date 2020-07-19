using System.Collections.Generic;
using System.Threading.Tasks;

namespace PataNext.Feature.RhythmEngineAudio.BGM.Directors
{
	public class TaskMap<TKey, TValue>
	{
		public delegate Task<TValue> OnTask(TKey key);

		private readonly Dictionary<TKey, TValue> cached;

		private readonly Dictionary<TKey, Task<TValue>> map;

		private readonly OnTask onTask;

		public TaskMap(OnTask onTask)
		{
			this.onTask = onTask;
			map         = new Dictionary<TKey, Task<TValue>>();
			cached      = new Dictionary<TKey, TValue>();
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

			map[key] = onTask(key);

			value = default;
			return false;
		}
	}
}
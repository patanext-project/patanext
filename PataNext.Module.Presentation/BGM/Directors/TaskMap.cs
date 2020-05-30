using System.Collections.Generic;
using System.Threading.Tasks;

namespace PataNext.Module.Presentation.BGM.Directors
{
	public class TaskMap<TKey, TValue>
	{
		public delegate Task<TValue> OnTask(TKey key);

		private Dictionary<TKey, Task<TValue>> map;

		private OnTask onTask;

		public TaskMap(OnTask onTask)
		{
			this.onTask = onTask;
			this.map    = new Dictionary<TKey, Task<TValue>>();
		}

		public bool GetValue(TKey key, out TValue value, out Task<TValue> task)
		{
			if (map.TryGetValue(key, out task))
			{
				if (task.IsCompleted)
				{
					value = task.Result;
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
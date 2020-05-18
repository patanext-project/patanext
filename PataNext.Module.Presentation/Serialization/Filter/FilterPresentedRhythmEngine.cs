using DefaultEcs;
using GameHost.Revolution.Public.Filters;
using PataNext.Module.Simulation.RhythmEngine;

namespace PataNext.Module.Presentation.Serialization.Filter
{
	public class FilterPresentedRhythmEngine : EntityParallelFilter
	{
		protected override void SetValidity(in Entity entity, ref bool invalid, ref bool valid)
		{
			if (entity.Has<RhythmEngineController>())
				valid = true;
		}
	}
}
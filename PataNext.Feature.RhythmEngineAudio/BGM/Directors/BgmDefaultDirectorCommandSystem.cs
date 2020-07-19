using GameHost.Core.Ecs;


namespace PataNext.Feature.RhythmEngineAudio.BGM.Directors
{
	public class BgmDefaultDirectorCommandSystem : BgmDirectorySystemBase<BgmDefaultDirector, BgmDefaultSamplesLoader>
	{
		public BgmDefaultDirectorCommandSystem(WorldCollection collection) : base(collection)
		{
		}
	}
}
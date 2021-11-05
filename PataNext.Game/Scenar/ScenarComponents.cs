using StormiumTeam.GameBase;

namespace PataNext.Game.Scenar
{
	public readonly struct ScenarLoadRequest
	{
		public readonly ResPath Path;

		public ScenarLoadRequest(ResPath path)
		{
			Path = path;
		}
	}
	
	public struct ScenarIsLoaded
	{
		
	}
}
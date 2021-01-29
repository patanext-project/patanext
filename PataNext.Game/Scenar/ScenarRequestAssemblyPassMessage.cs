using DefaultEcs;
using StormiumTeam.GameBase;

namespace PataNext.Game.Scenar
{
	public readonly struct ScenarRequestAssemblyPassMessage
	{
		public readonly ResPath ScenarPath;
		public readonly Entity  Entity;

		public ScenarRequestAssemblyPassMessage(Entity entity, ResPath resPath)
		{
			ScenarPath = resPath;
			Entity     = entity;
		}

		public void SetScenar(IScenar scenar)
		{
			Entity.Set(new ScenarResource {Interface = scenar});
		}
	}
}
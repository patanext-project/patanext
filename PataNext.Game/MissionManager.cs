using System.Collections.Generic;
using DefaultEcs;
using GameHost.Core.Ecs;
using StormiumTeam.GameBase;

namespace PataNext.Game
{
	public struct MissionDetails
	{
		public string  Name;
		public ResPath Scenar;
	}

	public class MissionManager : AppSystem
	{
		private Dictionary<ResPath, Entity> detailsMap = new();

		public MissionManager(WorldCollection collection) : base(collection)
		{
		}

		public void Register(ResPath missionPath, string name, ResPath scenarPath)
		{
			var ent = World.Mgr.CreateEntity();
			ent.Set(new MissionDetails
			{
				Name = name,
				Scenar = scenarPath
			});

			detailsMap[missionPath] = ent;
		}

		public bool TryGet(ResPath missionPath, out Entity entity)
		{
			return detailsMap.TryGetValue(missionPath, out entity);
		}
	}
}
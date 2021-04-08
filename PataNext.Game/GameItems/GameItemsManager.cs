using System.Collections.Generic;
using System.Diagnostics;
using DefaultEcs;
using GameHost.Core.Ecs;
using StormiumTeam.GameBase;

namespace PataNext.Game.GameItems
{
	public class GameItemsManager : AppSystem
	{
		private Dictionary<ResPath, Entity> idToEntityMap;

		public GameItemsManager(WorldCollection collection) : base(collection)
		{
			idToEntityMap = new();

			var helmEnt = collection.Mgr.CreateEntity();
			helmEnt.Set(new GameItemDescription("helm", "helm_desc"));
			helmEnt.Set(new EquipmentItemDescription
			{
				Additive = new()
				{
					Health = 60,
					Weight = 2
				}
			});
			
			Register(helmEnt, new(ResPath.EType.MasterServer, "st", "pn", "equipment/helm/default_helm"));
			
			var swordEnt = collection.Mgr.CreateEntity();
			swordEnt.Set(new GameItemDescription("sword", "sword_desc"));
			swordEnt.Set(new EquipmentItemDescription
			{
				Additive = new()
				{
					Attack = 12,
					Weight = 3
				}
			});
			
			Register(swordEnt, new(ResPath.EType.MasterServer, "st", "pn", "equipment/sword/default_sword"));
			
			var shieldEnt = collection.Mgr.CreateEntity();
			shieldEnt.Set(new GameItemDescription("shield", "shield_desc"));
			shieldEnt.Set(new EquipmentItemDescription
			{
				Additive = new()
				{
					Health = 25,
					Defense = 1,
					Weight = 1.5f
				}
			});
			
			Register(shieldEnt, new(ResPath.EType.MasterServer, "st", "pn", "equipment/shield/default_shield"));
			
			
			var spearEnt = collection.Mgr.CreateEntity();
			spearEnt.Set(new GameItemDescription("sword", "sword_desc"));
			spearEnt.Set(new EquipmentItemDescription
			{
				Additive = new()
				{
					Attack = 8,
					Weight = 3
				}
			});
			
			Register(spearEnt, new(ResPath.EType.MasterServer, "st", "pn", "equipment/spear/default_spear"));
			
			var bowEnt = collection.Mgr.CreateEntity();
			bowEnt.Set(new GameItemDescription("sword", "sword_desc"));
			bowEnt.Set(new EquipmentItemDescription
			{
				Additive = new()
				{
					Attack = 5,
					Weight = 1,
				}
			});
			
			Register(bowEnt, new(ResPath.EType.MasterServer, "st", "pn", "equipment/bow/default_bow"));
		}

		public void Register(Entity entity, ResPath resPath)
		{
			Debug.Assert(entity.Has<GameItemDescription>(), "entity.Has<GameItemDescription>()");

			idToEntityMap[resPath] = entity;
		}

		public bool TryGetDescription(ResPath resPath, out Entity entity)
		{
			return idToEntityMap.TryGetValue(resPath, out entity);
		}
	}
}
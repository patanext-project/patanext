using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Box2D.NetStandard.Collision.Shapes;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Injection.Dependency;
using GameHost.Simulation.Utility.Resource;
using PataNext.Game.Abilities;
using PataNext.Game.Abilities.Effects;
using PataNext.Game.GameItems;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.Army;
using PataNext.Module.Simulation.Components.GameModes;
using PataNext.Module.Simulation.Components.GameModes.City;
using PataNext.Module.Simulation.Components.GameModes.City.Scenes;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Network;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Hideout;
using PataNext.Module.Simulation.Game.Visuals;
using PataNext.Module.Simulation.GameModes;
using PataNext.Module.Simulation.Network.MasterServer.Services;
using PataNext.Module.Simulation.Network.MasterServer.Systems;
using PataNext.Module.Simulation.Resources;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Network.MasterServer.User;
using StormiumTeam.GameBase.Network.MasterServer.Utility;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.RuntimeTests.GameModes
{
	public class RuntimeTestUnitBarracks : GameAppSystem
	{
		GameResourceDb<UnitArchetypeResource> localArchetypeDb;
		GameResourceDb<UnitKitResource>       localKitDb;

		GameResourceDb<EquipmentResource>      equipDb;
		GameResourceDb<UnitAttachmentResource> attachDb;

		private GameResourceDb<GameGraphicResource> graphicDb;

		private ResPathGen                        resPathGen;
		private UnitStatusEffectComponentProvider statusEffectProvider;

		private MasterServerPlayerInventoryProvider inventoryProvider;

		private GameItemsManager itemMgr;
		
		private CurrentUserSystem currentUserSystem;

		private IPhysicsSystem physicsSystem;

		public RuntimeTestUnitBarracks(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref localArchetypeDb);
			DependencyResolver.Add(() => ref localKitDb);
			DependencyResolver.Add(() => ref equipDb);
			DependencyResolver.Add(() => ref attachDb);
			DependencyResolver.Add(() => ref graphicDb);

			DependencyResolver.Add(() => ref resPathGen);
			DependencyResolver.Add(() => ref statusEffectProvider);
			DependencyResolver.Add(() => ref itemMgr);
			
			DependencyResolver.Add(() => ref physicsSystem);
			
			DependencyResolver.Add(() => ref inventoryProvider);
			
			DependencyResolver.Add(() => ref currentUserSystem);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			DependencyResolver.AddDependency(new ConditionDependency(() => currentUserSystem.User.Token != null));
			DependencyResolver.OnComplete(deps =>
			{
				RequestUtility.CreateTracked(World.Mgr,
					new GetFavoriteGameSaveRequest(),
					(Entity _, GetFavoriteGameSaveRequest.Response response) => { startMasterserver(response.SaveId); });
			});
		}

		private void startMasterserver(string saveId)
		{
			var player = CreateEntity();
			AddComponent(Safe(player),
				new PlayerDescription(),
				new GameRhythmInputComponent(),
				new PlayerIsLocal(),
				new InputAuthority()
			);
			
			{
				var barracksScene = CreateEntity();
				AddComponent(barracksScene, new CityLocationTag());
				AddComponent(barracksScene, new CityBarrackScene());
				AddComponent(barracksScene, new Position(x: 12));
				AddComponent(barracksScene, new EntityVisual(graphicDb.GetOrCreate(resPathGen.Create(new[] { "Models", "GameModes", "City", "DefaultScenes", "Barracks" }, ResPath.EType.ClientResource))));

				using (var collider = World.Mgr.CreateEntity())
				{
					collider.Set((Shape)new Box2D.NetStandard.Collision.Shapes.PolygonShape(3, 2));
					physicsSystem.AssignCollider(barracksScene, collider);
				}
			}
			
			{
				var obeliskScene = CreateEntity();
				AddComponent(obeliskScene, new CityLocationTag());
				AddComponent(obeliskScene, new CityObeliskScene());
				AddComponent(obeliskScene, new Position(x: -12));
				AddComponent(obeliskScene, new EntityVisual(graphicDb.GetOrCreate(resPathGen.Create(new[] { "Models", "GameModes", "City", "DefaultScenes", "Obelisk" }, ResPath.EType.ClientResource))));

				using (var collider = World.Mgr.CreateEntity())
				{
					collider.Set((Shape)new Box2D.NetStandard.Collision.Shapes.PolygonShape(2, 5));
					physicsSystem.AssignCollider(obeliskScene, collider);
				}
			}

			AddComponent(CreateEntity(), new AtCityGameModeData());
			
			var inventory    = World.Mgr.CreateEntity();
			var inventoryObj = inventoryProvider.Create(saveId);
			inventory.Set(inventoryObj);
			inventory.Set((PlayerInventoryBase) inventoryObj);

			AddComponent(player, new PlayerAttachedGameSave(saveId));
			AddComponent(player, new PlayerInventoryTarget(inventory));

			var formation = CreateEntity();
			AddComponent(formation, new LocalArmyFormation());
		}
		
		private void startLocal(string saveId)
		{
			var uberArchResource = localArchetypeDb.GetOrCreate(new(resPathGen.Create(new[] { "archetype", "uberhero_std_unit" }, ResPath.EType.MasterServer)));
			var kitResource      = localKitDb.GetOrCreate(new("taterazay"));

			var player = CreateEntity();
			AddComponent(Safe(player),
				new PlayerDescription(),
				new GameRhythmInputComponent(),
				new PlayerIsLocal(),
				new InputAuthority()
			);
			
			var inventory    = World.Mgr.CreateEntity();
			/*var inventoryObj = new MasterServerPlayerInventory(saveId);
			inventory.Set(inventoryObj);
			inventory.Set((PlayerInventoryBase) inventoryObj);*/
			var inventoryObj = new LocalPlayerInventory();
			inventoryObj.ActionWorld = World.Mgr;
			inventory.Set(inventoryObj);
			inventory.Set((PlayerInventoryBase) inventoryObj);

			Entity helmItem = default;
			Entity swordItem = default;
			if (itemMgr.TryGetDescription(new(ResPath.EType.MasterServer, "st", "pn", new[] { "equipment", "helm", "default_helm" }), out var helmAsset))
				helmItem = inventoryObj.Create(helmAsset);
			if (itemMgr.TryGetDescription(new(ResPath.EType.MasterServer, "st", "pn", new[] { "equipment", "sword", "default_sword" }), out var swordAsset))
				swordItem = inventoryObj.Create(swordAsset);
			if (itemMgr.TryGetDescription(new(ResPath.EType.MasterServer, "st", "pn", new[] { "equipment", "spear", "default_spear" }), out var spearAsset))
			{
				inventoryObj.Create(spearAsset);
				inventoryObj.Create(spearAsset);
			}

			AddComponent(player, new PlayerAttachedGameSave(saveId));
			AddComponent(player, new PlayerInventoryTarget(inventory));
			
			GameWorld.AddComponent(player, AsComponentType<OwnedRelative<ArmySquadDescription>>());
			GameWorld.AddComponent(player, AsComponentType<OwnedRelative<ArmyUnitDescription>>());
			GameWorld.AddComponent(player, AsComponentType<OwnedRelative<UnitDescription>>());

			// ----
			// Create Army formation
			var formation = CreateEntity();
			AddComponent(Safe(formation),
				new ArmyFormationDescription());
			GameWorld.AddComponent(formation, AsComponentType<OwnedRelative<ArmySquadDescription>>());
			{
				// ----
				// Create a squad of units, with the player being a relative.
				var squad = CreateEntity();
				AddComponent(Safe(squad),
					new Relative<ArmyFormationDescription>(Safe(formation)),
					new Relative<PlayerDescription>(Safe(player)),
					new ArmySquadDescription(),
					new Owner(Safe(formation)));
				GameWorld.AddComponent(squad, AsComponentType<OwnedRelative<ArmyUnitDescription>>());
				{
					// ----
					// Add unit to army squad
					var unit = CreateEntity();
					AddComponent(Safe(unit),
						new Relative<ArmySquadDescription>(Safe(squad)),
						new Relative<ArmyFormationDescription>(Safe(formation)),
						new Relative<PlayerDescription>(Safe(player)),
						new ArmyUnitDescription(),
						new Owner(Safe(squad)),

						new UnitArchetype(uberArchResource),
						new UnitCurrentKit(kitResource),
						new UnitTargetControlTag()
					);

					AddComponent(unit, new UnitStatistics());

					var abilityBuffer = GameWorld.AddBuffer<UnitDefinedAbilities>(unit);
					abilityBuffer.Add(new("march", AbilitySelection.Horizontal));
					abilityBuffer.Add(new("backward", AbilitySelection.Horizontal));
					abilityBuffer.Add(new("party", AbilitySelection.Horizontal));
					abilityBuffer.Add(new(resPathGen.Create(new[] { "ability", "mega", "sonic_atk_def" }, ResPath.EType.MasterServer), AbilitySelection.Horizontal));
					abilityBuffer.Add(new(resPathGen.Create(new[] { "ability", "mega", "magic_atk_def" }, ResPath.EType.MasterServer), AbilitySelection.Bottom));
					abilityBuffer.Add(new(resPathGen.Create(new[] { "ability", "mega", "word_atk_def" }, ResPath.EType.MasterServer), AbilitySelection.Top));

					var displayedEquip = GameWorld.AddBuffer<UnitDisplayedEquipment>(unit);
					displayedEquip.Add(new()
					{
						Attachment = attachDb.GetOrCreate(resPathGen.Create(new[] { "equip_root", "mask" }, ResPath.EType.MasterServer)),
						Resource   = equipDb.GetOrCreate(resPathGen.Create(new[] { "equipments", "masks", "guardira" }, ResPath.EType.ClientResource))
					});
					displayedEquip.Add(new()
					{
						Attachment = attachDb.GetOrCreate(resPathGen.Create(new[] { "equip_root", "r_eq" }, ResPath.EType.MasterServer)),
						Resource   = equipDb.GetOrCreate(resPathGen.Create(new[] { "equipments", "horns", "default_horn" }, ResPath.EType.ClientResource))
					});

					var definedEquip = GameWorld.AddBuffer<UnitDefinedEquipments>(unit);
					definedEquip.Add(new()
					{
						Attachment = attachDb.GetOrCreate(resPathGen.Create(new[] { "equip_root", "helm" }, ResPath.EType.MasterServer)),
						Item   = helmItem
					});
					definedEquip.Add(new()
					{
						Attachment = attachDb.GetOrCreate(resPathGen.Create(new[] { "equip_root", "r_eq" }, ResPath.EType.MasterServer)),
						Item       = swordItem
					});

					var allowedEquip = GameWorld.AddBuffer<UnitAllowedEquipment>(unit);
					allowedEquip.Add(new()
					{
						Attachment = attachDb.GetOrCreate(resPathGen.Create(new[] { "equip_root", "helm" }, ResPath.EType.MasterServer)),
						EquipmentType = equipDb.GetOrCreate(resPathGen.Create(new[] { "item_type", "helm" }, ResPath.EType.MasterServer))
					});
					allowedEquip.Add(new()
					{
						Attachment    = attachDb.GetOrCreate(resPathGen.Create(new[] { "equip_root", "l_eq" }, ResPath.EType.MasterServer)),
						EquipmentType = equipDb.GetOrCreate(resPathGen.Create(new[] { "item_type", "shield" }, ResPath.EType.MasterServer))
					});
					allowedEquip.Add(new()
					{
						Attachment    = attachDb.GetOrCreate(resPathGen.Create(new[] { "equip_root", "r_eq" }, ResPath.EType.MasterServer)),
						EquipmentType = equipDb.GetOrCreate(resPathGen.Create(new[] { "item_type", "sword" }, ResPath.EType.MasterServer))
					});
					allowedEquip.Add(new()
					{
						Attachment    = attachDb.GetOrCreate(resPathGen.Create(new[] { "equip_root", "r_eq" }, ResPath.EType.MasterServer)),
						EquipmentType = equipDb.GetOrCreate(resPathGen.Create(new[] { "item_type", "spear" }, ResPath.EType.MasterServer))
					});

					statusEffectProvider.AddStatus(unit, AsComponentType<Critical>(), new() { Power = 12, Resistance = 30, RegenPerSecond = 0.1f });
					statusEffectProvider.AddStatus(unit, AsComponentType<Piercing>(), new() { Power = 0, Resistance = 20, RegenPerSecond = 0.2f });
					statusEffectProvider.AddStatus(unit, AsComponentType<KnockBack>(), new() { Power = 5, Resistance = 50, RegenPerSecond = 0.1f, ImmunityPerAttack = 0.3f });
					statusEffectProvider.AddStatus(unit, AsComponentType<Stagger>(), new() { Power = 5, Resistance = 50, RegenPerSecond = 0.1f, ImmunityPerAttack = 0.3f });
					statusEffectProvider.AddStatus(unit, AsComponentType<Burn>(), new() { Power = 5, Resistance = 50, RegenPerSecond = 0.1f, ImmunityPerAttack = 0.3f });
					statusEffectProvider.AddStatus(unit, AsComponentType<Sleep>(), new() { Power = 5, Resistance = 50, RegenPerSecond = 0.1f, ImmunityPerAttack = 0.3f });
					statusEffectProvider.AddStatus(unit, AsComponentType<Freeze>(), new() { Power = 5, Resistance = 50, RegenPerSecond = 0.1f, ImmunityPerAttack = 0.3f });
					statusEffectProvider.AddStatus(unit, AsComponentType<Poison>(), new() { Power = 999, Resistance = 999, RegenPerSecond = 1f, ImmunityPerAttack = 1f });
				}
			}
		}
	}
}
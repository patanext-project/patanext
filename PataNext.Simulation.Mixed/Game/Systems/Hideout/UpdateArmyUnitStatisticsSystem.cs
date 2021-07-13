using System;
using System.Collections.Generic;
using Collections.Pooled;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Native.Char;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using PataNext.Game.GameItems;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.Army;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Resources;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.Hideout
{
	// TODO: Should this be reactive? (perhaps adding a component like `DirtyStatistics`)
	[UpdateAfter(typeof(Game.GamePlay.Units.UnitCalculatePlayStateSystem))]
	public class UpdateArmyUnitStatisticsSystem : GameAppSystem
	{
		private UnitStatusEffectComponentProvider statusProvider;
		private GameItemsManager                  gameItemsManager;
		
		private readonly PooledList<ComponentReference> settingsRef;
		
		public UpdateArmyUnitStatisticsSystem([NotNull] WorldCollection collection) : base(collection)
		{
			AddDisposable(settingsRef = new PooledList<ComponentReference>());
			
			DependencyResolver.Add(() => ref statusProvider);
			DependencyResolver.Add(() => ref gameItemsManager);
		}

		private EntityQuery unitQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			unitQuery = CreateEntityQuery(new[]
			{
				typeof(ArmyUnitDescription),
				typeof(UnitStatistics),

				typeof(UnitDefinedEquipments)
			});
		}

		// let's see this big boi reaching 1giga of memory (actually not possible, it wouldn't even reach 1mega)
		private Dictionary<CharBuffer64, ResPath> bufferToResPath = new();

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var unitStatisticsAccessor    = GetAccessor<UnitStatistics>();
			var definedEquipmentsAccessor = GetBufferAccessor<UnitDefinedEquipments>();
			foreach (var entity in unitQuery)
			{
				settingsRef.Clear();
				GameWorld.GetComponentOf(entity, AsComponentType<StatusEffectSettingsBase>(), settingsRef);
				
				var length = settingsRef.Count;
				for (var i = 0; i < length; i++)
				{
					ref var effectSettings = ref GameWorld.GetComponentData<StatusEffectSettingsBase>(entity, settingsRef[i].Type);
					effectSettings.Power             = 0;
					effectSettings.Resistance        = 0;
					effectSettings.RegenPerSecond    = 0.1f;
					effectSettings.ImmunityPerAttack = 0;
				}

				ref var statistics = ref unitStatisticsAccessor[entity];
				statistics = new()
				{
					BaseWalkSpeed       = 2.0f,
					FeverWalkSpeed      = 2.2f,
					MovementAttackSpeed = 2.2f,
					Weight              = 4f,
					AttackSpeed         = 2.0f,
					AttackSeekRange     = 18f,

					AttackMeleeRange = 2f
				};

				var equipmentBuffer = definedEquipmentsAccessor[entity];
				foreach (var equip in equipmentBuffer)
				{
					var asset = equip.Item.Get<ItemInventory>().AssetEntity;
					if (gameItemsManager.TryGetDescription(asset.Get<GameItemDescription>().Id, out var equipmentDescEntity)
					    && equipmentDescEntity.TryGet(out EquipmentItemDescription equipmentDesc))
					{
						statistics.Health              += equipmentDesc.Additive.Health;
						statistics.Attack              += equipmentDesc.Additive.Attack;
						statistics.Defense             += equipmentDesc.Additive.Defense;
						
						statistics.Weight              += equipmentDesc.Additive.Weight;
						
						statistics.BaseWalkSpeed       += equipmentDesc.Additive.BaseWalkSpeed;
						statistics.FeverWalkSpeed      += equipmentDesc.Additive.FeverWalkSpeed;
						
						statistics.MovementAttackSpeed += equipmentDesc.Additive.MovementAttackSpeed;
						
						if (equipmentDesc.Additive.Status is { } statusMap)
							foreach (var (key, details) in statusMap)
							{
								if (!statusProvider.TryGetStatusType(key, out var componentType))
									continue;
									
								ref var settings = ref statusProvider.GetStatusSettings(entity, componentType);
								settings.Power += details.Power;
								settings.Resistance += details.Resistance;
							}
					}
				}
			}
		}
	}
}
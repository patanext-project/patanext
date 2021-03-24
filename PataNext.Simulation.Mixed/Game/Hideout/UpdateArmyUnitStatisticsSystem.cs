using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Native.Char;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using PataNext.Game.GameItems;
using PataNext.Module.Simulation.Components.Army;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Resources;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.Hideout
{
	public class UpdateArmyUnitStatisticsSystem : GameAppSystem
	{
		private GameItemsManager gameItemsManager;
		
		public UpdateArmyUnitStatisticsSystem([NotNull] WorldCollection collection) : base(collection)
		{
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
				ref var statistics = ref unitStatisticsAccessor[entity];
				statistics = new()
				{
					BaseWalkSpeed       = 2.0f,
					FeverWalkSpeed      = 2.2f,
					MovementAttackSpeed = 2.2f,
					Weight              = 4f,
					AttackSpeed         = 2.0f,
					AttackSeekRange     = 16f
				};

				var equipmentBuffer = definedEquipmentsAccessor[entity];
				foreach (var equip in equipmentBuffer)
				{
					var resource = GetComponentData<EquipmentResource>(equip.Resource.Handle);
					if (!bufferToResPath.TryGetValue(resource.Value, out var resPath))
						bufferToResPath[resource.Value] = resPath = new(resource.Value.ToString());

					if (gameItemsManager.TryGetDescription(resPath, out var equipmentDescEntity)
					    && equipmentDescEntity.TryGet(out EquipmentItemDescription equipmentDesc))
					{
						statistics.Health              += equipmentDesc.Additive.Health;
						statistics.Attack              += equipmentDesc.Additive.Attack;
						statistics.Defense             += equipmentDesc.Additive.Defense;
						
						statistics.Weight              += equipmentDesc.Additive.Weight;
						
						statistics.BaseWalkSpeed       += equipmentDesc.Additive.BaseWalkSpeed;
						statistics.FeverWalkSpeed      += equipmentDesc.Additive.FeverWalkSpeed;
						
						statistics.MovementAttackSpeed += equipmentDesc.Additive.MovementAttackSpeed;
					}
				}
			}
		}
	}
}
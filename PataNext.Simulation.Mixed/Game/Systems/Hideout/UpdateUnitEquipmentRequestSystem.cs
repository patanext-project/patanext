using System;
using DefaultEcs;
using GameHost.Core.Ecs;
using Microsoft.Extensions.Logging;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.Hideout;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using ZLogger;

namespace PataNext.Module.Simulation.Game.Hideout
{
	public class UpdateUnitEquipmentRequestSystem : GameAppSystem
	{
		private EntitySet requestSet;
		private ILogger   logger;

		public UpdateUnitEquipmentRequestSystem(WorldCollection collection) : base(collection)
		{
			requestSet = collection.Mgr.GetEntities()
			                       .With<UpdateUnitEquipmentRequest>()
			                       .AsSet();

			DependencyResolver.Add(() => ref logger);
		}

		public override bool CanUpdate() => base.CanUpdate() && requestSet.Count > 0;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			foreach (ref readonly var requestEntity in requestSet.GetEntities())
			{
				var request = requestEntity.Get<UpdateUnitEquipmentRequest>();
				if (!request.Entity.Exists())
					continue;

				if (!request.Entity.Has<Relative<PlayerDescription>>())
				{
					logger.ZLogWarning($"{request.Entity.Entity} has no Relative<PlayerDescription>");
					continue;
				}

				var relative = request.Entity.GetData<Relative<PlayerDescription>>().Target;
				if (!TryGetComponentData(relative, out PlayerInventoryTarget inventoryTarget))
				{
					logger.ZLogError($"Player '{relative}' has no inventory");
					continue;
				}

				var inventory = inventoryTarget.Value.Get<PlayerInventoryBase>();
				if (inventory is ISwapEquipmentInventory swapEquipmentInventory)
				{
					foreach (var (attachment, itemEntity) in request.Updates)
						swapEquipmentInventory.RequestSwap(request.Entity, attachment, itemEntity);
				}
			}

			requestSet.DisposeAllEntities();
		}
	}
}
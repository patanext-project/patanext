using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Structures.Bastion;
using PataNext.Module.Simulation.Components.Roles;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.Providers
{
	public class BastionFixedGroupProvider : BaseProvider<BastionFixedGroupProvider.Create>
	{
		public struct Create
		{
			public GameEntity[] Entities;

			/// <summary>
			/// AutoLink <see cref="Entities"/> to the newly created bastion entity
			/// </summary>
			public bool? AutoLink;
		}

		public BastionFixedGroupProvider([NotNull] WorldCollection collection) : base(collection)
		{
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			entityComponents.AddRange(new[]
			{
				AsComponentType<BastionDescription>(),
				AsComponentType<BastionEntities>(),
			});
		}

		public override void SetEntityData(GameEntityHandle entity, Create data)
		{
			var buffer = GetBuffer<BastionEntities>(entity).Reinterpret<GameEntity>();
			buffer.AddRange(data.Entities);

			if (data.AutoLink == true)
				foreach (var unit in buffer)
					GameWorld.Link(unit.Handle, entity, true);

			foreach (var unit in buffer)
				GameWorld.UpdateOwnedComponent(unit.Handle, new Relative<BastionDescription>(Safe(entity)));
		}
	}
}
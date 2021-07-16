using System;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Structures.Bastion;
using PataNext.Module.Simulation.Components.Roles;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.Providers
{
	public class BastionDynamicGroupProvider : BaseProvider<BastionDynamicGroupProvider.Create>
	{
		public struct Create
		{
			/// <summary>
			/// How many units will be created.
			/// </summary>
			/// <remarks>
			///	If more than 0, it will use the first <see cref="Dents"/>
			/// </remarks>
			public int SpawnCount;

			public int? MaxEntities; // by default it will use SpawnCount

			public bool? GiveOwnershipOfDents;

			public Entity[] Dents;

			public void SetProviderForAll<TProvider, TCreate>(TProvider provider, TCreate create, int spawnCount)
				where TProvider : BaseProvider<TCreate>
				where TCreate : struct
			{
				GiveOwnershipOfDents = true;

				Dents    = new Entity[1];
				Dents[0] = provider.World.Mgr.CreateEntity();

				Dents[0].Set<BaseProvider>(provider);
				Dents[0].Set(create);

				SpawnCount = spawnCount;
			}

			public void SetProviderForAll<TProvider, TCreate>(TProvider[] provider, TCreate[] create)
				where TProvider : BaseProvider<TCreate>
				where TCreate : struct
			{
				GiveOwnershipOfDents = true;

				if (provider.Length != create.Length)
					throw new InvalidOperationException();

				Dents = new Entity[provider.Length];
				for (var i = 0; i < Dents.Length; i++)
				{
					Dents[i] = provider[i].World.Mgr.CreateEntity();

					Dents[i].Set<BaseProvider>(provider[i]);
					Dents[i].Set(create[i]);
				}

				SpawnCount = 0;
			}
		}

		public BastionDynamicGroupProvider([NotNull] WorldCollection collection) : base(collection)
		{
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			entityComponents.AddRange(new[]
			{
				AsComponentType<BastionDescription>(),
				AsComponentType<BastionEntities>(),
				AsComponentType<BastionProvideDynamicEntity>(),
				AsComponentType<BastionSettings>()
			});
		}

		public override void SetEntityData(GameEntityHandle entity, Create data)
		{
			var maxEntities = data.MaxEntities ?? data.SpawnCount;

			GetComponentData<BastionSettings>(entity).DynamicMaxUnits = maxEntities;

			if (data.GiveOwnershipOfDents == true)
				foreach (var ent in data.Dents)
					ent.Set<BastionProvideDynamicEntity.HasDentOwnerShip>();

			var board = BastionProvideDynamicEntity.GetComponentBoard(Focus(Safe(entity)), out var metadata);
			board.DentMap[metadata.Id] = data.Dents;

			GameWorld.GetComponentData<BastionProvideDynamicEntity>(entity).SpawnCount                      = data.SpawnCount;
			GameWorld.GetComponentData<BastionProvideDynamicEntity>(entity).AssureRemoveBastionUnitWhenDead = true;
		}
	}
}
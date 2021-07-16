using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Components.GamePlay.Structures.Bastion
{
	public struct BastionProvideDynamicEntity : IComponentData, IMetadataCustomComponentBoard
	{
		public int  SpawnCount;
		public bool AssureRemoveBastionUnitWhenDead;

		public struct HasDentOwnerShip {}
		
		public static Board GetComponentBoard(SafeEntityFocus focus, out EntityBoardContainer.ComponentMetadata metadata)
		{
			var world         = focus.GameWorld;
			var componentType = world.AsComponentType<BastionProvideDynamicEntity>();

			var board = world.GetComponentBoard<Board>(componentType);

			metadata = world.GetComponentMetadata(focus.Handle, componentType);
			if (metadata.Null)
				throw new InvalidOperationException(world.DebugCreateErrorMessage(focus.Handle, $"{nameof(BastionProvideDynamicEntity)} not found"));

			return board;
		}
		
		public ComponentBoardBase ProvideComponentBoard(GameWorld gameWorld)
		{
			return new Board(0);
		}

		public class Board : SingleComponentBoard
		{
			public Entity[][] DentMap      = Array.Empty<Entity[]>();

			public Board(int capacity) : base(Unsafe.SizeOf<BastionProvideDynamicEntity>(), capacity)
			{
			}
			
			protected override void OnResize()
			{
				base.OnResize();

				var previousLength = DentMap.Length;
				Array.Resize(ref DentMap, (int)board.MaxId + 1);
				for (var i = previousLength; i < DentMap.Length; i++)
					DentMap[i] = Array.Empty<Entity>();
			}

			public override bool DeleteRow(uint row)
			{
				var deleted = base.DeleteRow(row);
				if (deleted)
				{
					foreach (var dent in DentMap[row])
						if (dent.Has<HasDentOwnerShip>())
							dent.Dispose();
				}

				return deleted;
			}
		}
	}
}
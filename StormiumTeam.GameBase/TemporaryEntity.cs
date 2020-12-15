using System;
using GameHost.Simulation.TabEcs;

namespace StormiumTeam.GameBase
{
	public struct TemporaryEntity : IDisposable
	{
		private readonly GameWorld  gameWorld;
		private readonly GameEntity entity;

		public TemporaryEntity(GameWorld gameWorld)
		{
			entity         = gameWorld.Safe(gameWorld.CreateEntity());
			this.gameWorld = gameWorld;
		}

		public static implicit operator GameEntity(TemporaryEntity       temporaryEntity) => temporaryEntity.entity;
		public static implicit operator GameEntityHandle(TemporaryEntity temporaryEntity) => temporaryEntity.entity.Handle;

		public void Dispose()
		{
			if (gameWorld.Safe(entity.Handle).Version != entity.Version)
				throw new InvalidOperationException("You destroyed a temporary entity.");
			gameWorld.RemoveEntity(entity.Handle);
		}
	}
}
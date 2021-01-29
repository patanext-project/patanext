using System;
using System.Runtime.CompilerServices;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace StormiumTeam.GameBase.SystemBase
{
	public partial class GameAppSystem
	{
		public SafeEntityFocus Focus(GameEntity entity)
		{
			return new SafeEntityFocus(GameWorld, entity);
		}
	}

	public struct SafeEntityFocus
	{
		public readonly GameWorld        GameWorld;
		public readonly GameEntity       Entity;
		public readonly GameEntityHandle Handle;

		public bool Exists() => GameWorld.Exists(Entity);

		public void ThrowIfNotExists()
		{
			if (Exists() == false)
				throw new InvalidOperationException($"{Entity} does not exist!");
		}

		public SafeEntityFocus(GameWorld gameWorld, GameEntity entity)
		{
			GameWorld = gameWorld;
			Entity    = entity;
			Handle    = entity.Handle;
		}
		
		public bool Has(ComponentType componentType)
		{
			return GameWorld.HasComponent(Handle, componentType);
		}

		public bool Has<T>()
			where T : struct, IEntityComponent
		{
			return GameWorld.HasComponent<T>(Handle);
		}

		public SafeEntityFocus Add(ComponentType componentType)
		{
			ThrowIfNotExists();

			GameWorld.AddComponent(Handle, componentType);
			return this;
		}

		public SafeEntityFocus AddData<T>(T component = default)
			where T : struct, IComponentData
		{
			ThrowIfNotExists();

			GameWorld.AddComponent(Handle, component);
			return this;
		}

		public ComponentBuffer<T> AddBuffer<T>()
			where T : struct, IComponentBuffer
		{
			ThrowIfNotExists();

			return GameWorld.AddBuffer<T>(Handle);
		}

		public ref T GetData<T>()
			where T : struct, IComponentData
		{
			return ref GameWorld.GetComponentData<T>(Handle);
		}

		public ComponentBuffer<T> GetBuffer<T>()
			where T : struct, IComponentBuffer
		{
			return GameWorld.GetBuffer<T>(Handle);
		}
	}
}
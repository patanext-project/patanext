using System;
using GameHost.Injection;
using GameHost.Native.Char;
using GameHost.Native.Fixed;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using GameHost.Simulation.Utility.Resource.Interfaces;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;
using PataNext.Module.Simulation.Game.RhythmEngine;
using PataNext.Module.Simulation.Resources.Keys;

namespace PataNext.Module.Simulation.Resources
{
	public struct RhythmCommandResource : IGameResourceDescription
	{

	}

	public readonly struct RhythmCommandIdentifier : IComponentData
	{
		public readonly CharBuffer64 Value;

		public RhythmCommandIdentifier(CharBuffer64 value) => Value = value;
	}

	public class RhythmCommandResourceDb : GameResourceDb<RhythmCommandResource, Keys.RhythmCommandResourceKey>
	{
		public RhythmCommandResourceDb(Context context) : base(context)
		{
		}

		public RhythmCommandResourceDb(GameWorld gameWorld) : base(gameWorld)
		{
		}

		public GameResource<RhythmCommandResource> GetOrCreate(ComponentType type, string identifier = null, RhythmCommandAction[] buffer = null)
		{
			var resource = GetOrCreate(new RhythmCommandResourceKey(type));
			if (!GameWorld.HasComponent(resource.Entity, type))
				GameWorld.AddComponent(resource.Entity, type);

			Console.WriteLine($"Register command {identifier} as {type.Id} on resource {resource.Entity}");

			if (identifier != null)
				GameWorld.AddComponent(resource.Entity, new RhythmCommandIdentifier(identifier));

			if (buffer != null)
				GameWorld.AddBuffer<RhythmCommandActionBuffer>(resource.Entity).AddRangeReinterpret(buffer.AsSpan());

			return resource;
		}
	}
}
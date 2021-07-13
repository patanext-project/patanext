using System;
using System.Threading;
using GameHost.Injection;
using GameHost.Native.Char;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using GameHost.Simulation.Utility.Resource.Components;
using GameHost.Simulation.Utility.Resource.Interfaces;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;
using PataNext.Module.Simulation.Game.RhythmEngine;

namespace PataNext.Module.Simulation.Resources
{
	public readonly struct RhythmCommandResource : IGameResourceDescription, IEquatable<RhythmCommandResource>
	{
		public readonly ComponentType Identifier;

		public class Register : RegisterGameHostComponentData<RhythmCommandResource>
		{
		}

		public RhythmCommandResource(ComponentType identifier)
		{
			Identifier = identifier;
		}

		public bool Equals(RhythmCommandResource other)
		{
			return Identifier.Equals(other.Identifier);
		}

		public override bool Equals(object obj)
		{
			return obj is RhythmCommandResource other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Identifier.GetHashCode();
		}
	}

	public readonly struct RhythmCommandIdentifier : IComponentData
	{
		public readonly CharBuffer64 Value;
		public readonly int          Duration;

		public RhythmCommandIdentifier(CharBuffer64 value, int duration)
		{
			Value    = value;
			Duration = duration;
		}
	}

	public class RhythmCommandResourceDb : GameResourceDb<RhythmCommandResource>
	{
		public RhythmCommandResourceDb(Context context) : base(context)
		{
		}

		public RhythmCommandResourceDb(GameWorld gameWorld) : base(gameWorld)
		{
		}

		public GameResource<RhythmCommandResource> GetOrCreate(ComponentType type, string identifier = null, RhythmCommandAction[] buffer = null, int? beatDuration = null)
		{
			var resource = GetOrCreate(new RhythmCommandResource(type));
			if (!GameWorld.HasComponent(resource.Handle, type))
				GameWorld.AddComponent(resource.Handle, type);

			if (beatDuration == null && buffer != null)
			{
				beatDuration = 0;
				foreach (var element in buffer)
					beatDuration = Math.Max(beatDuration.Value, (int) Math.Ceiling(element.Beat.Target + 1 + element.Beat.Offset + element.Beat.SliderLength));
			}

			if (identifier != null)
				GameWorld.AddComponent(resource.Handle, new RhythmCommandIdentifier(identifier, beatDuration ?? 4));

			if (buffer != null)
				GameWorld.AddBuffer<RhythmCommandActionBuffer>(resource.Handle).AddRangeReinterpret(buffer.AsSpan());

			return resource;
		}
	}
}
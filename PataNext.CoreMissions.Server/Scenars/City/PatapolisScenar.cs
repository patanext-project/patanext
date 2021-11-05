using System;
using System.Numerics;
using System.Threading.Tasks;
using Box2D.NetStandard.Collision.Shapes;
using GameHost.Core.Ecs;
using GameHost.IO;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using PataNext.CoreMissions.Mixed.Missions.City;
using PataNext.Game.Scenar;
using PataNext.Module.Simulation.Components.GameModes.City;
using PataNext.Module.Simulation.Components.GameModes.City.Scenes;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Game.Visuals;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreMissions.Server.Scenars.City
{
	public class PatapolisScenar : ScenarScript
	{
		public class Provider : ScenarProvider
		{
			public Provider(WorldCollection collection) : base(collection)
			{
			}

			public override ResPath ScenarPath => PatapolisCityRegister.ScenarPath;
			public override IScenar Provide()  => new PatapolisScenar(World);
		}

		public PatapolisScenar(WorldCollection wc) : base(wc)
		{
		}

		protected override Task OnStart()
		{
			GameEntityHandle CreateScene(Vector2 position = default, string graphic = default, Shape colliderShape = default)
			{
				var handle = CreateEntity();
				AddComponent(handle, new CityLocationTag());
				AddComponent(handle, new Position(position.X, position.Y));
				AddComponent(handle, new EntityVisual(GraphicDb.GetOrCreate(new(graphic))));

				using var colliderSettings = World.Mgr.CreateEntity();
				colliderSettings.Set(colliderShape);
				PhysicsSystem.AssignCollider(handle, colliderSettings);

				GameWorld.Link(handle, stackalloc[] { Self.Handle, Creator.Handle }, true);

				return handle;
			}

			var barracksScene = CreateScene(
				new(10, 0),
				ResPathGen.Create(new[] { "Models", "GameModes", "City", "DefaultScenes", "Barracks" }, ResPath.EType.ClientResource),
				new PolygonShape(3, 2));
			AddComponent(barracksScene, new CityBarrackScene());

			var obeliskScene = CreateScene(
				new(-10, 0),
				ResPathGen.Create(new[] { "Models", "GameModes", "City", "DefaultScenes", "Obelisk" }, ResPath.EType.ClientResource),
				new PolygonShape(2, 10));
			AddComponent(obeliskScene, new CityObeliskScene());

			Console.WriteLine("loaded patapolis scenar!");
			return Task.CompletedTask;
		}

		protected override Task OnLoop()
		{
			return Task.CompletedTask;
		}

		protected override Task OnCleanup(bool reuse)
		{
			Console.WriteLine("destroy patapolis scenar!");
			return Task.CompletedTask;
		}
	}
}
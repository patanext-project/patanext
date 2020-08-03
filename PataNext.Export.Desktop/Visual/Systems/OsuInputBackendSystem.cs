using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Inputs.Components;
using GameHost.Inputs.Layouts;
using GameHost.Inputs.Systems;

namespace PataNext.Export.Desktop.Visual.Systems
{
	[RestrictToApplication(typeof(IOsuFrameworkApplication))]
	public class OsuInputBackendSystem : AppSystem
	{
		private InputBackendSystem                  inputBackendSystem;
		private OsuInputBackendRegisterActionSystem                  registerActionSystem;

		public OsuInputBackendSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref inputBackendSystem);
			DependencyResolver.Add(() => ref registerActionSystem);
		}

		public InputControl GetInputControl(string path)
		{
			return inputBackendSystem.GetInputControl(path);
		}

		public void ClearCurrentActions()
		{
			using (var set = World.Mgr.GetEntities().With<ReplicatedInputAction>().AsSet())
				set.DisposeAllEntities();
			inputBackendSystem.ghIdToEntityMap.Clear();
		}
		internal Entity RegisterAction(TransportConnection connection, string ghActionType, InputEntityId action)
		{
			var entity = registerActionSystem.TryGetCreateActionBase(ghActionType);
			if (entity == default)
				return default;

			var repl = new ReplicatedInputAction
			{
				Connection = connection,
				Id         = action.Value
			};
			entity.Set(repl);
			entity.Set(action);

			inputBackendSystem.ghIdToEntityMap[repl] = entity;
			inputBackendSystem.ghIdToLayoutsMap[repl] = new InputActionLayouts();

			return entity;
		}
	}
}
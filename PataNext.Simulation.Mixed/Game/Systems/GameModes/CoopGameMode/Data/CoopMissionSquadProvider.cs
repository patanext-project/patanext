using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.Army;
using PataNext.Module.Simulation.Components.Roles;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.GameModes.DataCoopMission
{
	public class CoopMissionSquadProvider : BaseProvider<CoopMissionSquadProvider.Create>
	{
		public struct Create
		{
			public float Offset;
		}

		public CoopMissionSquadProvider([NotNull] WorldCollection collection) : base(collection)
		{
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			entityComponents.AddRange(new[]
			{
				AsComponentType<InGameSquadDescription>(),
				AsComponentType<SquadEntityContainer>(),
				AsComponentType<InGameSquadOffset>()
			});
		}

		public override void SetEntityData(GameEntityHandle entity, Create data)
		{
			GetComponentData<InGameSquadOffset>(entity).Value = data.Offset;
		}
	}
}
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Game.GamePlay.Damage;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.CoreAbilities.Server
{
	public class SetStatusEffectBufferHelper : GameAppSystem
	{
		private PooledList<ComponentReference> statusRefs;

		public SetStatusEffectBufferHelper([NotNull] WorldCollection collection) : base(collection)
		{
			AddDisposable(statusRefs = new PooledList<ComponentReference>());
		}

		public void Set(GameEntityHandle source, GameEntityHandle to)
		{
			if (!GameWorld.HasComponent<DamageFrameDataStatusEffect>(to))
				GameWorld.AddBuffer<DamageFrameDataStatusEffect>(to);

			var statusEffectBuffer = GameWorld.GetBuffer<DamageFrameDataStatusEffect>(to);
			{
				statusEffectBuffer.Clear();
				statusRefs.Clear();
				GameWorld.GetComponentOf(source, GameWorld.AsComponentType<StatusEffectStateBase>(), statusRefs);
				foreach (var componentRef in statusRefs)
				{
					ref readonly var state = ref GameWorld.GetComponentData<StatusEffectStateBase>(source, componentRef.Type);
					statusEffectBuffer.Add(new DamageFrameDataStatusEffect(state.Type, state.CurrentPower));
				}
			}
		}
	}
}
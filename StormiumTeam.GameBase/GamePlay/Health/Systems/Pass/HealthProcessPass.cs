using System;
using GameHost.Core.Ecs.Passes;

namespace StormiumTeam.GameBase.GamePlay.Health.Systems.Pass
{
	public interface IHealthProcessPass
	{
		void OnTrigger(Span<ModifyHealthEvent> events);
	}

	public class RegisterHealthProcessPass : PassRegisterBase<IHealthProcessPass>
	{
		public HealthSystem HealthSystem { get; internal set; }

		public RegisterHealthProcessPass()
		{
			ManualTrigger = true;
		}
		
		protected override void OnTrigger()
		{
			foreach (var obj in GetObjects())
			{
				if (obj is IUpdatePass updatePass && !updatePass.CanUpdate())
					continue;

				obj.OnTrigger(HealthSystem.HealthEvents);
			}
		}
	}
}
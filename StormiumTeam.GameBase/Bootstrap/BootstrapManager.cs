using System;
using System.Collections.Generic;
using GameHost.Applications;
using GameHost.Core.Ecs;

namespace StormiumTeam.GameBase.Bootstrap
{
	[RestrictToApplication(typeof(ExecutiveEntryApplication))]
	public class BootstrapManager : AppSystem
	{
		public BootstrapManager(WorldCollection collection) : base(collection)
		{
		}

		private Dictionary<string, (string display, Action<string> action)> bootstraps = new();

		public void AddEntry(string id, string displayName, Action<string> onExecute)
		{
			bootstraps[id] = (displayName, onExecute);
		}

		public void Execute(string id, string jsonArgs)
		{
			bootstraps[id].action(jsonArgs);
		}
	}
}
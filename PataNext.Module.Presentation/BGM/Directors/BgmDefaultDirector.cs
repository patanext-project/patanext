using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Collections.Pooled;
using GameHost.Core.Bindables;
using GameHost.Core.IO;

namespace PataNext.Module.Presentation.BGM.Directors
{
	public class BgmDefaultDirector : BgmDirectorBase
	{
		public readonly Bindable<IncomingCommand> IncomingCommandBindable;

		private Dictionary<int, int> commandCycle;

		public BgmDefaultDirector(JsonElement elem, BgmStore store, BgmDirectorBase parent) : base(elem, store, parent)
		{
			Loader                  = new BgmDefaultSamplesLoader(store);
			IncomingCommandBindable = new Bindable<IncomingCommand>();

			commandCycle = new Dictionary<int, int>();
		}

		public int GetNextCycle(string commandId, string state)
		{
			if (!(Loader.GetCommand(commandId) is BgmDefaultSamplesLoader.ComboBasedCommand command))
				return 0;

			var hash = commandId.GetHashCode();
			if (!commandCycle.ContainsKey(hash))
				commandCycle.Add(hash, -1);
			
			var cycle = commandCycle[hash] + 1;
			if (cycle >= command.mappedFile[state].Count)
				cycle = 0;

			commandCycle[hash] = cycle;
			return cycle;
		}

		public struct IncomingCommand
		{
			public string   CommandId;
			public TimeSpan Start, End;
		}
	}
}
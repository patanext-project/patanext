﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using GameHost.Core;
 using GameHost.Native.Char;
 using GameHost.Simulation.Utility.Resource;
 using PataNext.Module.Simulation.Resources;

 namespace PataNext.Feature.RhythmEngineAudio.BGM.Directors
{
	public class BgmDefaultDirector : BgmDirectorBase
	{
		public readonly Bindable<IncomingCommandData> IncomingCommand;
		public readonly Bindable<bool>                IsFever;

		private readonly Dictionary<int, int> commandCycle;

		public BgmDefaultDirector(JsonElement elem, BgmStore store, BgmDirectorBase parent) : base(elem, store, parent)
		{
			Loader          = new BgmDefaultSamplesLoader(store);
			IncomingCommand = new Bindable<IncomingCommandData>();
			IsFever         = new Bindable<bool>();

			commandCycle = new Dictionary<int, int>();
		}

		public int GetNextCycle(CharBuffer64 commandId, string state)
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

		public struct IncomingCommandData
		{
			public GameResource<RhythmCommandResource> CommandId;
			public TimeSpan                            Start, End;
		}
	}
}
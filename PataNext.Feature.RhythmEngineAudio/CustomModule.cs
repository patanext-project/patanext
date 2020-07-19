using System;
using DefaultEcs;
using GameHost.Core.Modules;
using GameHost.Injection;

namespace PataNext.Feature.RhythmEngineAudio
{
	public class CustomModule : GameHostModule
	{
		public CustomModule(Entity source, Context ctxParent, GameHostModuleDescription original) : base(source, ctxParent, original)
		{
			foreach (var file in DllStorage.GetFilesAsync("Sounds/RhythmEngine/*.*").Result) 
				Console.WriteLine(file.FullName);
		}
	}
}
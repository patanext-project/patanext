using System;

namespace PataNext.Game
{
	public abstract class ScriptRunner : IDisposable
	{
		public abstract void SetAndLoad(Span<byte> content);
		
		public abstract void Dispose();
	}

	public class CSharpScriptRunner : ScriptRunner
	{
		public override void SetAndLoad(Span<byte> content)
		{
			
		}

		public override void Dispose()
		{
			
		}
	}
}
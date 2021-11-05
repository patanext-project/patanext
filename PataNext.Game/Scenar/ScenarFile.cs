using System.Threading.Tasks;
using GameHost.Core.IO;

namespace PataNext.Game.Scenar
{
	public class ScenarFile : IFile
	{
		public string       Name     { get; }
		public string       FullName { get; }
		public Task<byte[]> GetContentAsync()
		{
			throw new System.NotImplementedException();
		}
	}
}
using System.Collections.Generic;
using System.Threading.Tasks;
using GameHost.Core.IO;

namespace PataNext.Game.BGM
{
	/// <summary>
	/// A bgm container contains multiple BGMs
	/// </summary>
	public class BgmContainerStorage : IStorage
	{
		private IStorage parent;

		public BgmContainerStorage(IStorage parent)
		{
			this.parent = parent;
		}

		public string CurrentPath => parent.CurrentPath;

		public Task<IEnumerable<IFile>> GetFilesAsync(string             pattern) => parent.GetFilesAsync(pattern);
		public Task<IStorage>           GetOrCreateDirectoryAsync(string path)    => parent.GetOrCreateDirectoryAsync(path);
		
		public async IAsyncEnumerable<BgmFile> GetBgmAsync(string pattern)
		{
			var files = await GetFilesAsync(pattern);
			foreach (var f in files)
			{
				var bgm = new BgmFile(f);
				await bgm.ComputeDescription();

				yield return bgm;
			}
		}
	}
}
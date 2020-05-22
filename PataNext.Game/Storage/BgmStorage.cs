using System.Collections.Generic;
using System.Threading.Tasks;
using GameHost.Core.IO;

namespace PataponGameHost.Storage
{
	public class BgmStorage : IStorage
	{
		private IStorage parent;

		public BgmStorage(IStorage parent)
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
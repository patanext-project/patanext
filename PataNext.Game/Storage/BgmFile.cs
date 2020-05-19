using System.Text.Json;
using System.Threading.Tasks;
using GameHost.Core.IO;
using GameHost.IO;

namespace PataponGameHost.Storage
{
	public class BgmFile : IFile
	{
		private readonly IFile parent;

		public BgmDescription Description { get; private set; }

		public string Name => parent.Name;
		public string FullName => parent.FullName;

		public Task<byte[]> GetContentAsync() => parent.GetContentAsync();

		public BgmFile(IFile parent)
		{
			this.parent = parent;
		}

		public async Task ComputeDescription()
		{
			var       fileContent = await GetContentAsync();
			using var document    = JsonDocument.Parse(fileContent);

			var root = document.RootElement;

			Description = new BgmDescription
			{
				Id          = root.GetProperty("id").GetString(),
				Name        = root.GetProperty("name").GetString(),
				Author      = root.GetProperty("author").GetString(),
				Description = root.GetProperty("description").GetString(),
			};
		}
	}

	public struct BgmDescription
	{
		public string Id;
		public string Name;
		public string Author;
		public string Description;
	}

	public class BgmResource : Resource
	{
	}
}
using System.IO.Enumeration;
using ICSharpCode.SharpZipLib.Zip;
using Quadrum.Game.BGM;
using revghost.IO.Storage;

namespace PataNext.Game.Client.RhythmEngineAudio.BGM;

	/// <summary>
	///     A bgm store contain the required files for a BGM.
	/// </summary>
	public abstract class BgmStore : IStorage
	{
		public readonly BgmFile Source;

		public BgmStore(BgmFile source)
		{
			Source = source;
		}

		public abstract void GetFiles<TList>(string pattern, TList listToFill) where TList : IList<IFile>;

		public IStorage GetSubStorage(string path)
		{
			throw new NotImplementedException("BgmStore does not implement directory searching and creation of directories.");
		}

		public abstract string CurrentPath { get; }
		
		public static async Task<BgmStore> Create(BgmFile file)
		{
			if (file.Description.StorePath == null)
				await file.ComputeDescription();
			if (file.Description.StorePath.StartsWith("zip://"))
				return new BgmZipStore(file);
			if (file.Description.StorePath.StartsWith("relative://"))
				return new BgmLocalStore(file, true);
			if (file.Description.StorePath.StartsWith("file://"))
				return new BgmLocalStore(file, false);

			throw new NotImplementedException("no store implemented for " + file.Description.StorePath);
		}
	}

	internal class BgmZipStore : BgmStore
	{
		public BgmZipStore(BgmFile source) : base(source)
		{
			CurrentPath = source.FullName;
		}

		public override void GetFiles<TList>(string pattern, TList listToFill)
		{
			using (var stream = new MemoryStream(Source.GetPooledBytes().ToArray()))
			using (var zip = new ZipFile(stream))
			{
				foreach (ZipEntry entry in zip)
					if (FileSystemName.MatchesSimpleExpression(pattern, entry.Name))
						listToFill.Add(new ZipEntryFile(Source, entry));
			}
		}

		public override string CurrentPath { get; }
	}

internal class BgmLocalStore : BgmStore
{
	private readonly string computedPath;
	private readonly bool isRelative;

	private readonly LocalStorage storage;

	public BgmLocalStore(BgmFile source, bool isRelative) : base(source)
	{
		this.isRelative = isRelative;

		if (isRelative)
			// Yes, the store path start with relative://, but this is a way to add a slash between parent directory and bgm directory
			computedPath = source.Description.StorePath.Replace("relative:/", Path.GetDirectoryName(source.FullName));
		else
			computedPath = source.Description.StorePath.Replace("file://", string.Empty);

		var directory = new DirectoryInfo(computedPath);
		if (!directory.Exists)
			throw new DirectoryNotFoundException(directory.FullName);

		storage = new LocalStorage(directory);
	}

	public override string CurrentPath => computedPath;
	
	public override void GetFiles<TList>(string pattern, TList listToFill)
	{
		storage.GetFiles(pattern, listToFill);
	}
}
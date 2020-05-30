using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using GameHost.Core.IO;
using PataNext.Module.Presentation.BGM.Directors;
using PataponGameHost.Storage;

namespace PataNext.Module.Presentation.BGM
{
	public static class BgmDirector
	{
		public static async Task<BgmDirectorBase> Create(BgmFile file)
		{
			if (file.DirectorJson.ValueKind == JsonValueKind.Undefined)
			{
				await file.ComputeDescription();
			}

			var type = typeof(BgmDefaultDirector);
			if (file.DirectorJson.TryGetProperty("type", out var typeProperty))
				type = Type.GetType(typeProperty.GetString());

			if (type == null && typeProperty.GetString() != null)
				throw new InvalidOperationException($"Invalid 'director' [{typeProperty.GetString()}] type for file {file.FullName}");

			return (BgmDirectorBase) Activator.CreateInstance(type, file.DirectorJson, await BgmStore.Create(file), null);
		}
	}

	public abstract class BgmDirectorBase
	{
		public BgmSamplesLoaderBase Loader { get; protected set; }

		public BgmDirectorBase(JsonElement elem, BgmStore store, BgmDirectorBase parent)
		{
		}
	}

	public abstract class BgmSamplesLoaderBase
	{
		protected BgmStore Store { get; set; }

		public abstract class BFile
		{
			public abstract Task<IEnumerable<IFile>> PreloadFiles();
		}

		public abstract class BCommand : BFile
		{
			public readonly string Id;

			public BCommand(string id) => this.Id = id;
		}

		public abstract class BSoundTrack : BFile
		{
		}

		public abstract BCommand    GetCommand(string commandId);
		public abstract BSoundTrack GetSoundtrack();
		public abstract BFile       GetFile<TFileDescription>(TFileDescription description) where TFileDescription : BFileDescription;

		public BgmSamplesLoaderBase(BgmStore store)
		{
			Store = store;
		}
	}
}
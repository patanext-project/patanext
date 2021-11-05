using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using GameHost.Core.IO;
using GameHost.Native.Char;
using PataNext.Feature.RhythmEngineAudio.BGM.Directors;
using PataNext.Game.BGM;

namespace PataNext.Feature.RhythmEngineAudio.BGM
{
	public static class BgmDirector
	{
		public static async Task<BgmDirectorBase> Create(BgmFile file)
		{
			if (file.DirectorJson.ValueKind == JsonValueKind.Undefined) await file.ComputeDescription();

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
		public BgmDirectorBase(JsonElement elem, BgmStore store, BgmDirectorBase parent)
		{
		}

		public BgmSamplesLoaderBase Loader { get; protected set; }
	}

	public abstract class BgmSamplesLoaderBase
	{
		public BgmSamplesLoaderBase(BgmStore store)
		{
			Store = store;
		}

		protected BgmStore Store { get; set; }

		public abstract BCommand    GetCommand(CharBuffer64 commandId);
		public abstract BSoundTrack GetSoundtrack();
		public abstract BFile       GetFile<TFileDescription>(TFileDescription description) where TFileDescription : BFileDescription;

		public abstract class BFile
		{
			public abstract Task<IEnumerable<IFile>> PreloadFiles();
		}

		public abstract class BCommand : BFile
		{
			public readonly CharBuffer64 Id;

			public BCommand(CharBuffer64 id)
			{
				Id = id;
			}
		}

		public abstract class BSoundTrack : BFile
		{
		}
	}
}
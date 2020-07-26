﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
 using Collections.Pooled;
 using GameHost.Core.IO;
 using GameHost.Native.Char;

 namespace PataNext.Feature.RhythmEngineAudio.BGM.Directors
{
	public class BgmDefaultSamplesLoader : BgmSamplesLoaderBase
	{
		private readonly TaskMap<CharBuffer64, ComboBasedCommand> commandsTaskMap;
		private readonly TaskMap<string, SingleFile>        filesTaskMap;

		public BgmDefaultSamplesLoader(BgmStore store) : base(store)
		{
			commandsTaskMap = new TaskMap<CharBuffer64, ComboBasedCommand>(async key =>
			{
				var files = (await Store.GetFilesAsync($"commands/{key}/*.ogg"))
				            .Concat(await Store.GetFilesAsync($"commands/{key}/*.wav"))
				            .ToArray();

				if (files.Length == 0)
					throw new InvalidOperationException($"No files found for command {key}");

				return new ComboBasedCommand(files, key);
			});
			filesTaskMap = new TaskMap<string, SingleFile>(async key =>
			{
				var files = (await Store.GetFilesAsync($"samples/{key}.ogg"))
				            .Concat(await Store.GetFilesAsync($"samples/{key}.wav"))
				            .ToArray();

				if (files.Length == 0)
					throw new InvalidOperationException($"No files found for sample '{key}'");

				return new SingleFile(files.First());
			});
		}

		public override BCommand GetCommand(CharBuffer64 commandId)
		{
			if (!commandsTaskMap.GetValue(commandId, out var command, out var task)) return null;

			if (task.Exception != null)
				throw task.Exception;

			return command;
		}

		public override BSoundTrack GetSoundtrack()
		{
			return null;
		}

		public override BFile GetFile<TFileDescription>(TFileDescription description)
		{
			string sampleId = null;
			switch (description)
			{
				case BFileOnEnterFeverSoundDescription desc:
					sampleId = "enter_fever";
					break;
				case BFileOnFeverLostSoundDescription desc:
					sampleId = "fever_lost";
					break;
				case BFileSampleDescription desc:
					sampleId = desc.SampleName;
					break;
			}

			if (sampleId != null && filesTaskMap.GetValue(sampleId, out var file, out var task))
			{
				if (task.Exception != null)
					throw task.Exception;
				return file;
			}

			return null;
		}

		public class ComboBasedCommand : BCommand
		{
			private readonly IFile[] files;

			public Dictionary<string, PooledList<IFile>> mappedFile;

			public ComboBasedCommand(IFile[] files, CharBuffer64 id) : base(id)
			{
				this.files = files;

				mappedFile = new Dictionary<string, PooledList<IFile>>
				{
					{"normal", new PooledList<IFile>()},
					{"prefever", new PooledList<IFile>()},
					{"fever", new PooledList<IFile>()}
				};
				foreach (var f in files)
				{
					var nameWithoutExt = Path.GetFileNameWithoutExtension(f.Name);

					var key                                                         = Regex.Match(nameWithoutExt, "[^_]+").Value;
					if (!mappedFile.TryGetValue(key, out var list)) mappedFile[key] = list = new PooledList<IFile>(2);

					if (int.TryParse(Regex.Match(nameWithoutExt, "[^_]+$").Value, out var i)
					    && i < list.Count)
						list.Insert(i, f);
					else
						list.Add(f);
				}
			}

			public ReadOnlySpan<IFile> Normal   => mappedFile["normal"].Span;
			public ReadOnlySpan<IFile> PreFever => mappedFile["prefever"].Span;
			public ReadOnlySpan<IFile> Fever    => mappedFile["fever"].Span;

			public override async Task<IEnumerable<IFile>> PreloadFiles()
			{
				return files;
			}
		}

		private struct CommandTask
		{
			public Task<ComboBasedCommand> Task;
		}

		public class SlicedSoundTrack : BSoundTrack
		{
			public override async Task<IEnumerable<IFile>> PreloadFiles()
			{
				return new List<IFile>();
			}
		}

		public class SingleFile : BFile
		{
			public IFile File;

			public SingleFile(IFile file)
			{
				File = file;
			}

			public override async Task<IEnumerable<IFile>> PreloadFiles()
			{
				return new[] {File};
			}
		}
	}
}
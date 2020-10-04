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
		private readonly TaskMap<byte, SlicedSoundTrack> bgmTaskMap;
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
			bgmTaskMap = new TaskMap<byte, SlicedSoundTrack>(async key =>
			{
				var files = (await Store.GetFilesAsync($"soundtrack/*.ogg"))
				            .Concat(await Store.GetFilesAsync($"soundtrack/*.wav"))
				            .ToArray();

				if (files.Length == 0)
					throw new InvalidOperationException($"No files found for soundtrack");

				return new SlicedSoundTrack(files);
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
			if (!bgmTaskMap.GetValue(0, out var soundTrack, out var task)) return null;

			if (task.Exception != null)
				throw task.Exception;

			return soundTrack;
		}

		public override BFile GetFile<TFileDescription>(TFileDescription description)
		{
			var sampleId = description switch
			{
				BFileOnEnterFeverSoundDescription desc => "enter_fever",
				BFileOnFeverLostSoundDescription desc => "fever_lost",
				BFileSampleDescription desc => desc.SampleName,
				_ => null
			};

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
			private readonly IFile[] files;

			public Dictionary<string, PooledList<IFile>> mappedFile;

			public SlicedSoundTrack(IFile[] files)
			{
				this.files = files;

				mappedFile = new Dictionary<string, PooledList<IFile>>
				{
					{"before_entrance", new PooledList<IFile>()},
					{"before", new PooledList<IFile>()},
					{"fever_entrance", new PooledList<IFile>()},
					{"fever_", new PooledList<IFile>()}
				};
				foreach (var f in files)
				{
					var nameWithoutExt = Path.GetFileNameWithoutExtension(f.Name);

					var key   = string.Empty;
					var split = nameWithoutExt.Split("_");
					for (var i = 0; i != split.Length - 1; i++)
					{
						if (i > 0)
							key += "_";
						key += split[i];
					}

					if (!mappedFile.TryGetValue(key, out var list)) mappedFile[key] = list = new PooledList<IFile>(2);

					if (int.TryParse(Regex.Match(nameWithoutExt, "[^_]+$").Value, out var idx))
					{
						if (list.Count < idx)
							list.AddSpan(idx - list.Count);
					}

					if (idx >= 0)
						list.Insert(idx, f);
					else
						list.Add(f);
				}

				foreach (var list in mappedFile.Values)
				{
					list.RemoveAll(f => f is null);
				}
			}

			public ReadOnlySpan<IFile> BeforeEntrance => mappedFile["before_entrance"].Span;
			public ReadOnlySpan<IFile> Before         => mappedFile["before"].Span;
			public ReadOnlySpan<IFile> FeverEntrance  => mappedFile["fever_entrance"].Span;
			public ReadOnlySpan<IFile> Fever          => mappedFile["fever"].Span;

			public override async Task<IEnumerable<IFile>> PreloadFiles()
			{
				return files;
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
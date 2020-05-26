using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Collections.Pooled;
using GameHost.Core.Bindables;
using GameHost.Core.IO;

namespace PataNext.Module.Presentation.BGM.Directors
{
	public class BgmDefaultSamplesLoader : BgmSamplesLoaderBase
	{
		private readonly Dictionary<string, ComboBasedCommand> commandsResultMap;
		private readonly Dictionary<string, CommandTask>       commandsTaskMap;

		public BgmDefaultSamplesLoader(BgmStore store) : base(store)
		{
			commandsResultMap = new Dictionary<string, ComboBasedCommand>(12);
			commandsTaskMap   = new Dictionary<string, CommandTask>(12);
		}

		public override BCommand GetCommand(string commandId)
		{
			if (commandsResultMap.TryGetValue(commandId, out var command))
			{
				return command;
			}

			if (commandsTaskMap.TryGetValue(commandId, out var ct))
			{
				if (!ct.Task.IsCompleted || ct.Task.Exception != null)
				{
					if (ct.Task.Exception != null)
						throw ct.Task.Exception;
					return null;
				}

				commandsResultMap[commandId] = ct.Task.Result;
				return ct.Task.Result;
			}

			ct = new CommandTask
			{
				Task = GetCommandForId(commandId)
			};

			commandsTaskMap[commandId] = ct;
			if (ct.Task.IsCompletedSuccessfully)
				return ct.Task.Result;
			return null;
		}

		private async Task<ComboBasedCommand> GetCommandForId(string commandId)
		{
			var files = (await Store.GetFilesAsync($"commands/{commandId}/*.ogg"))
			            .Concat(await Store.GetFilesAsync($"commands/{commandId}/*.wav"))
			            .ToArray();

			if (files.Length == 0)
				throw new InvalidOperationException($"No files found for command {commandId}");

			return new ComboBasedCommand(files, commandId);
		}

		public override BSoundTrack GetSoundtrack()
		{
			return null;
		}

		public class ComboBasedCommand : BCommand
		{
			private readonly IFile[] files;

			public Dictionary<string, PooledList<IFile>> mappedFile;

			public ComboBasedCommand(IFile[] files, string id) : base(id)
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
	}
}
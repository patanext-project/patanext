using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using GameHost.Applications;
using GameHost.Core.Client;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(ExecutiveEntryApplication))]
	public class AddConsoleAsRpcClient : AppSystem
	{
		private Thread                thread;
		private StartGameHostListener listener;

		private ConcurrentQueue<string> linesToExecute;
		private CancellationTokenSource cancellationTokenSource;

		public AddConsoleAsRpcClient(WorldCollection collection) : base(collection)
		{
			thread = new(() =>
			{
				while (!cancellationTokenSource.IsCancellationRequested)
				{
					var str = Console.ReadLine();
					if (string.IsNullOrEmpty(str))
						return;
					
					if (str.StartsWith("rpc "))
						linesToExecute.Enqueue(str.Replace("rpc ", string.Empty));
				}
			});
			linesToExecute          = new();
			cancellationTokenSource = new();

			AddDisposable(cancellationTokenSource);

			DependencyResolver.Add(() => ref listener);
		}

		private EventBasedNetListener eventListener = new EventBasedNetListener();
		private NetManager            netManager;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			
			listener.Server.Subscribe((previous, next) =>
			{
				if (next == null)
					return;

				var port = next.LocalPort;
				netManager ??= new(eventListener);
				if (!netManager.IsRunning)
					netManager.Start();
					
				netManager?.DisconnectAll();
				netManager.Connect("127.0.0.1", port, string.Empty);
			}, true);
			
			thread.Start();
			eventListener.NetworkReceiveEvent += (peer, reader, method) =>
			{
				var rpcType = reader.GetString();
				var cmdType  = reader.GetString();
				if (cmdType == nameof(RpcCommandType.Reply))
				{
					var cmd    = reader.GetString();
					var buffer = new DataBufferReader(reader.GetRemainingBytesSegment());
					if (buffer.Length > 0)
					{
						Console.WriteLine($"(RPC) Command '{cmd}' reply\n{buffer.ReadString()}");
					}
					else
					{
						Console.WriteLine($"(RPC) Command '{cmd}' with empty data received!");
					}
				}
			};
		}

		public override bool CanUpdate()
		{
			return base.CanUpdate() && netManager != null;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			netManager.PollEvents();
			while (linesToExecute.TryDequeue(out var str))
			{
				var split = str.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
				if (split.Length < 1)
					continue;

				if (string.IsNullOrEmpty(split[0]))
					continue;
				
				var data = new NetDataWriter();
				data.Put(nameof(RpcMessageType.Command));
				data.Put(nameof(RpcCommandType.Send));
				data.Put(split[0]);
				
				if (split.Length == 2 && !string.IsNullOrEmpty(split[1]))
				{
					using var writer = new DataBufferWriter(split.Length * 2);
					writer.WriteStaticString(split[1]);
					data.Put(writer.Span.ToArray());
				}
				netManager.SendToAll(data, DeliveryMethod.ReliableOrdered);
			}
		}

		public override void Dispose()
		{
			base.Dispose();

			if (netManager != null && netManager.IsRunning)
				netManager.DisconnectAll();
		}
	}
}
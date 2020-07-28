using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Net.NetworkInformation;
using System.Threading;
using GameHost.Applications;
using GameHost.Core.Ecs;
using ZetaIpc.Runtime.Server;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(ExecutiveEntryApplication))]
	public class AddReceiveGameHostClientFeature : AppSystem
	{
		public IpcServer Server { get; }

		public AddReceiveGameHostClientFeature(WorldCollection collection) : base(collection)
		{
			AddDisposable(Server = new IpcServer {Port = 10_950});
		}

		protected override void OnInit()
		{
			base.OnInit();
			
			Server.Start();
			Server.ReceivedRequest += (obj, args) =>
			{
				Console.WriteLine($"{args.Request}");
				args.Handled = true;
			};
		}
	}
}
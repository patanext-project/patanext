﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using DryIoc;
using GameHost.Applications;
using GameHost.Core.IO;
using GameHost.Injection;
using PataponGameHost.Storage;

namespace PataponGameHost
{
	public static class GameAppShared
	{
		/// <summary>
		/// Start initialization shared code between applications (may wait)
		/// </summary>
		/// <param name="context">The context to use</param>
		/// <param name="disposableObjects">Get a list with objects to dispose when application close</param>
		public static void Init(Context context, out List<IDisposable> disposableObjects)
		{
			var currentStorage = context.Container.Resolve<IStorage>();
			context.Bind(new BgmStorage(currentStorage.GetOrCreateDirectoryAsync("Bgm").Result));
			Debug.Assert(context.Container.Resolve<BgmStorage>() != null, "context.Container.Resolve<BgmStorage>() != null");

			disposableObjects = new List<IDisposable>();
			
			var inputThread = new GameInputThreadingHost(context);
			inputThread.Listen();

			var simulationThread = new GameSimulationThreadingHost(1f / 100);
			simulationThread.Listen();
			
			disposableObjects.Add(inputThread);
			disposableObjects.Add(simulationThread);
		}
	}
}
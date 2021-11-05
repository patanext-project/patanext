using System;
using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osuTK;
using PataNext.Export.Desktop.Visual;

namespace PataNext.Export.Desktop.Tests
{
	public class TestGameBrowser : VisualGameBase
	{
		protected override Container<Drawable> Content { get; }

		public TestGameBrowser()
		{
			// Ensure game and tests scale with window size and screen DPI.
			base.Content.Add(Content = new DrawSizePreservingFillContainer
			{
				// You may want to change TargetDrawSize to your "default" resolution, which will decide how things scale and position when using absolute coordinates.
				TargetDrawSize = new Vector2(1366, 768)
			});
		}

		protected override void LoadComplete()
		{
			base.LoadComplete();
			
			AddRange(new Drawable[]
			{
				new TestBrowser("PataNext"),
				new CursorContainer()
			});

			/*Assembly wA = null;
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Console.WriteLine(assembly.FullName);
				if (assembly.FullName?.Contains("Win32") == true)
					wA = assembly;
			}

			if (wA != null)
			{
				Console.WriteLine("WindowsAssembly Found: " + wA.FullName);
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (assembly != wA)
					{
						foreach (var asm in assembly.GetReferencedAssemblies())
							if (asm.Name.Contains(wA.GetName().Name) || asm.Name.Contains("Windows") || asm.Name.Contains("Win32"))
								Console.WriteLine($"\t{assembly.FullName} contains wA ({asm.Name})");
					}
				}
				
				foreach (var refe in wA.GetReferencedAssemblies())
					Console.WriteLine("reference to " + refe.FullName);
			}*/
		}

		public override void SetHost(osu.Framework.Platform.GameHost host)
		{
			base.SetHost(host);
			host.Window.CursorState |= CursorState.Hidden;
		}
	}
}
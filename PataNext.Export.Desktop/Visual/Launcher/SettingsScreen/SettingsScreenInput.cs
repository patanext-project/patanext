using System;
using System.IO;
using System.Linq;
using System.Text;
using GameHost.Core.IO;
using GameHost.Core.Modules;
using GameHost.Game;
using GameHost.Inputs.Systems;
using GameHost.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK;
using PataNext.Export.Desktop.Visual.Configuration;
using PataNext.Export.Desktop.Visual.Overlays;
using PataNext.Simulation.Client.Systems.Inputs;
using SharpInputSystem;

namespace PataNext.Export.Desktop.Visual.SettingsScreen
{
	public class SettingsScreenInput : SettingsScreenBase
	{
		public SettingsScreenInput()
		{

		}

		[CanBeNull] private IStorage inputStorage;


		private InputManager inputManager;
		private Keyboard     kb;

		private            DependencyContainer          dependencies;
		protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
		{
			return dependencies = new (parent);
		}

		[BackgroundDependencyLoader(true)]
		private void load([CanBeNull] GameBootstrap bootstrap, InputManager inputManager)
		{
			if (inputManager is null)
				throw new NullReferenceException(nameof(inputManager));

			this.inputManager                                                       = inputManager;
			(kb = inputManager.CreateInputObject<Keyboard>(true, "")).EventListener = new SharpDxInputSystem.KeyboardListenerSimple();

			dependencies.Cache(kb.EventListener as SharpDxInputSystem.KeyboardListenerSimple);

			if (bootstrap != null)
			{
				var module = new PataNext.Simulation.Client.Module(default, bootstrap.Global.Context, new()
				{
					DisplayName = "TestModule",
					Author      = "Test",
					NameId      = typeof(PataNext.Simulation.Client.Module).Assembly.GetName().Name
				});

				module.Storage.Subscribe((_, curr) =>
				{
					if (curr == null)
						return;
					inputStorage = new StorageCollection()
					{
						module.DllStorage,
						curr,
					}.GetOrCreateDirectoryAsync("Inputs").Result;
					Scheduler.Add(() => createRhythm());
				}, true);
			}
			else
				createRhythm();
		}

		[CanBeNull]
		private IWriteFile getFile<T>()
		{
			IFile      readableFile = null;
			IWriteFile writableFile = null;
			if (inputStorage != null)
			{
				foreach (var file in inputStorage.GetFilesAsync($"{typeof(T).Name}.json").Result)
				{
					readableFile = file;

					if (file is IWriteFile)
						writableFile = file as IWriteFile;
				}

				// We need to create one
				if (writableFile == null && readableFile != null)
				{
					var writableStorage = (inputStorage as StorageCollection)!.AllStorage.Last() as LocalStorage;
					if (writableStorage == null)
						throw new InvalidOperationException("the last storage should be a localstorage");

					using var file = File.Create($"{writableStorage.CurrentPath}/{typeof(T).Name}.json");
					file.Write(readableFile.GetContentAsync().Result);
				}

				if (writableFile == null && readableFile == null)
					throw new InvalidOperationException("No input file found");
			}

			return writableFile;
		}

		private T deserialize<T>(byte[] data)
		{
			using var stream = new MemoryStream(data);
			using var reader = new JsonTextReader(new StreamReader(stream));

			var serializer = new JsonSerializer();
			return serializer.Deserialize<T>(reader);
		}

		private Bindable<RhythmInputDescription> rhythmInputDescription = new();
		private void createRhythm()
		{
			var file = getFile<RhythmInputDescription>();
			if (file is {} readableFile)
			{
				rhythmInputDescription.Value = deserialize<RhythmInputDescription>(readableFile.GetContentAsync().Result);
			}

			ReactiveInputButton<RhythmInputDescription> provideButton(ReactiveInputButton<RhythmInputDescription>.func getProperty)
			{
				return new()
				{
					GetProperty = getProperty,
					Bindable    = rhythmInputDescription,

					Size   = new(200, 30),
					Action = () => Console.WriteLine("woo")
				};
			}

			var category = "Rhythm Session (Drums)";
			AddSetting("Pata (Left)", provideButton((ref RhythmInputDescription input) => ref input.PataKeys), category: category);
			AddSetting("Pon (Right)", provideButton((ref RhythmInputDescription input) => ref input.PonKeys), category: category);
			AddSetting("Don (Down)", provideButton((ref  RhythmInputDescription input) => ref input.DonKeys), category: category);
			AddSetting("Chaka (Up)", provideButton((ref  RhythmInputDescription input) => ref input.ChakaKeys), category: category);

			category = "Rhythm Session (Abilities)";
			AddSetting("Horizontal", provideButton((ref RhythmInputDescription input) => ref input.Ability0Keys), category: category);
			AddSetting("Up", provideButton((ref RhythmInputDescription input) => ref input.Ability1Keys), category: category);
			AddSetting("Down", provideButton((ref  RhythmInputDescription input) => ref input.Ability2Keys), category: category);

			category = "InGame (Camera)";
			AddSetting("Panning Left", provideButton((ref RhythmInputDescription input) => ref input.PanningNegativeKeys), category: category);
			AddSetting("Panning Right", provideButton((ref RhythmInputDescription input) => ref input.PanningPositiveKeys), category: category);
			
			rhythmInputDescription.BindValueChanged(ev =>
			{
				if (file is null)
					return;

				file.WriteContentAsync(Encoding.Default.GetBytes(JsonConvert.SerializeObject(ev.NewValue)));
			});
		}

		protected override void Update()
		{
			foreach (var kvp in (kb.EventListener as SharpDxInputSystem.KeyboardListenerSimple).ControlMap)
				kvp.Value.IsPressed = false;
			
			kb.Capture();
			
			base.Update();
		}

		protected override void Dispose(bool isDisposing)
		{
			base.Dispose(isDisposing);
			
			kb?.Dispose();
		}
	}

	public class ReactiveInputButton<TInput> : BasicButton
	{
		public delegate ref string[] func(ref TInput input);

		public Bindable<TInput> Bindable;

		public func GetProperty;
		
		public ReactiveInputButton()
		{
			BackgroundColour = FrameworkColour.BlueGreen;
		}

		protected override void LoadComplete()
		{
			base.LoadComplete();
			
			Bindable.BindValueChanged(ev =>
			{
				var v = Bindable.Value;
				foreach (var p in GetProperty(ref v))
				{
					Text = p;
					break;
				}

				isBinding        = false;
				BackgroundColour = FrameworkColour.BlueGreen;
			}, true);
		}

		private SharpDxInputSystem.KeyboardListenerSimple kb;
		
		[BackgroundDependencyLoader]
		private void load(SharpDxInputSystem.KeyboardListenerSimple kb)
		{
			this.kb = kb;
		}

		protected override void Update()
		{
			if (isBinding)
			{
				foreach (var (_, input) in kb.ControlMap)
				{
					if (input.IsPressed)
					{
						Console.WriteLine(input.Id);
						
						var     v     = Bindable.Value;
						ref var array = ref GetProperty(ref v);
						array = array.Length == 0 ? new string[1] : array.ToArray(); // clone (so that the bindable can get updated)
						
						array[0]       = input.Id;
						
						Bindable.Value = v;
						
						isBinding = false;
						break;
					}
				}
			}
			
			base.Update();
		}

		private bool isBinding;
		protected override bool OnClick(ClickEvent e)
		{
			Text             = "Waiting for new binding";
			BackgroundColour = FrameworkColour.BlueGreenDark;

			isBinding = true;
			return true;
		}
	}
}
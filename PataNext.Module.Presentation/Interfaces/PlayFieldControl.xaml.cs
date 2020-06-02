using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using GameHost.Applications;
using GameHost.Core.Threading;
using GameHost.UI.Noesis;
using Noesis;
using PataNext.Module.Presentation.BGM;
using PataponGameHost.Applications.MainThread;
using PataponGameHost.Storage;

namespace PataNext.Module.Presentation.Controls
{
	public class PlayFieldControl : LoadableUserControl<PlayFieldControl.ViewModel>
	{
		public FrameworkElement BeatImpulse;

		private SolidColorBrush beatImpulseBrush = new SolidColorBrush();

		public Color BeatImpulseColor
		{
			get => beatImpulseBrush.Color;
			set
			{
				if (!beatImpulseBrush.Color.Equals(value))
				{
					beatImpulseBrush.Color        = value;
					Resources["BeatImpulseColor"] = beatImpulseBrush;
				}
			}
		}

		public override void OnLoad()
		{
			try
			{
				BeatImpulse = FindName<FrameworkElement>("BeatImpulseFrame");
				GenContext.EnabledTrails = new ObservableCollection<bool>(new bool[3] {true, false, true});
				
				BeatImpulseColor = Color.FromScRgb(1, 0.75f, 0.75f, 0.75f);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		public override void OnUnload()
		{
		}

		public override void Dispose()
		{
		}

		public class ViewModel : NotifyPropertyChangedBase
		{
			public TimeSpan AccumulatedBeatTime;
			public TimeSpan AccumulatedBeatTimeExp;

			private ObservableCollection<bool> enabledTrails;

			public ObservableCollection<bool> EnabledTrails
			{
				get => enabledTrails;
				set
				{
					if (value == null || enabledTrails?.SequenceEqual(value) == true)
						return;

					enabledTrails = new ObservableCollection<bool>(value);
					OnPropertyChanged(nameof(EnabledTrails));
				}
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using GameHost.Applications;
using GameHost.Core.Threading;
using GameHost.UI.Noesis;
using ImTools;
using Noesis;
using PataNext.Module.Presentation.BGM;
using PataNext.Module.RhythmEngine;
using PataponGameHost.Applications.MainThread;
using PataponGameHost.Storage;

namespace PataNext.Module.Presentation.Controls
{
	public class PlayFieldControl : LoadableUserControl<PlayFieldControl.ViewModel>
	{
		public class OuterEllipse : Grid
		{
			public Ellipse Inner;
			public Ellipse Outer;

			public OuterEllipse(Ellipse inner, Ellipse outer)
			{
				this.Inner = inner;
				this.Outer = outer;

				Width  = inner.Width * 2;
				Height = inner.Height * 2;

				HorizontalAlignment = HorizontalAlignment.Center;
				VerticalAlignment = VerticalAlignment.Center;
				
				inner.HorizontalAlignment = HorizontalAlignment.Center;
				inner.VerticalAlignment   = VerticalAlignment.Center;
				
				outer.HorizontalAlignment = HorizontalAlignment.Center;
				outer.VerticalAlignment   = VerticalAlignment.Center;

				Children.Add(outer);
				Children.Add(inner);
			}
		}

		public FrameworkElement BeatImpulse;
		public Grid IncomingBeatsGrid;

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
				IncomingBeatsGrid = FindName<Grid>("IncomingBeats");

				var intervalBox = FindName<TextBox>("IntervalBox");
				intervalBox.TextChanged += (sender, args) =>
				{
					Console.WriteLine(intervalBox.Text);
				};

				KeyDown += (sender, args) =>
				{
					switch (args.Key)
					{
						case Key.Enter:
						{
							if (intervalBox.IsFocused)
							{
								if (!float.TryParse(intervalBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
								&& !float.TryParse(intervalBox.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
								{
									Console.WriteLine("Invalid value. " + value);
									return;
								}

								if (!ThreadingHost.TryGetListener(out GameSimulationThreadingHost simulationHost))
								{
									Console.WriteLine("Listener not found");
									return;
								}

								simulationHost.GetScheduler().Add(() =>
								{
									foreach (var (instance, world) in simulationHost.MappedWorldCollection)
									{
										foreach (ref var component in world.Mgr.Get<RhythmEngineSettings>())
										{
											component.BeatInterval = TimeSpan.FromSeconds(value);
										}
									}
								});
							}
						
							Keyboard.Focus(null);
							break;
						}
						case Key.Escape:
							Keyboard.Focus(null);
							break;
					}
				};
				
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

		public void OnNewBeat(TimeSpan time, Color color)
		{
			var inner = new Ellipse
			{
				Width               = 40, Height                                    = 40,
				HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
				Fill                = new SolidColorBrush(Color.FromScRgb(1, 0.1f, 0.1f, 0.1f))
			};
			var outer = new Ellipse
			{
				Width               = 0, Height                                     = 0,
				HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
				Fill                = new SolidColorBrush(Color.FromScRgb(1, 0.3f, 0.3f, 0.3f))
			};
			var quad = new OuterEllipse(inner, outer);
			
			quad.SetValue(IsBeatProperty, true);
			quad.SetValue(TimeProperty, time);
			
			IncomingBeatsGrid.Children.Add(quad);
		}

		public void OnNewPressure(TimeSpan time, Color color)
		{
			var ellipse = new Ellipse
			{
				Width  = 40,
				Height = 40,
				Fill   = new SolidColorBrush(color)
			};
			var grid = new Grid {Width = 80, Height = 80};
			grid.Children.Add(ellipse);
			
			grid.SetValue(TimeProperty, time);
			
			IncomingBeatsGrid.Children.Add(grid);
		}

		protected override bool EnableFrameUpdate()
		{
			return true;
		}

		public TimeSpan BeatInterval;
		public TimeSpan Elapsed;

		public override void OnUpdate()
		{
			var width = ActualWidth;
			IncomingBeatsGrid.Margin = new Thickness(0, 0, -width, 0);
			
			var delta = (float) Delta.TotalSeconds;
			for (var index = 0; index < IncomingBeatsGrid.Children.Count; index++)
			{
				var children    = (FrameworkElement) IncomingBeatsGrid.Children[index];
				var margin = children.Margin;
				var isBeat = (bool) children.GetValue(IsBeatProperty);
				var time = (TimeSpan) children.GetValue(TimeProperty);
				
				margin.Right = (float)(width * (Elapsed.TotalSeconds - time.TotalSeconds));
				if (!isBeat)
					margin.Right += width;

				children.Margin = margin;

				if (Math.Abs(margin.Right * 0.5f) > width * 0.5f)
				{
					if ((bool) children.GetValue(IsHalvedProperty) == false && isBeat)
					{
						var widthAnimation = new DoubleAnimation {Duration = new Duration(TimeSpan.FromSeconds(1f)), To = 0};
						Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(Shape.WidthProperty));
						
						var heightAnimation = new DoubleAnimation {Duration = new Duration(TimeSpan.FromSeconds(1f)), To = 0};
						Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(Shape.HeightProperty));

						var oe = (OuterEllipse) children;
						
						oe.Outer.Width = oe.Inner.Width + 30;
						oe.Outer.Height = oe.Inner.Height + 30;
						oe.Outer.BeginAnimation(Shape.WidthProperty, widthAnimation);
						oe.Outer.BeginAnimation(Shape.HeightProperty, heightAnimation);

						children.SetValue(IsHalvedProperty, true);
					}

					children.Opacity -= delta;
				}

				if (Math.Abs(margin.Right * 0.5f) > width * 2)
				{
					IncomingBeatsGrid.Children.RemoveAt(index--);
				}
			}
		}
		
		private static DependencyProperty IsBeatProperty = DependencyProperty.Register("IsBeat", typeof(bool), typeof(Ellipse));
		private static DependencyProperty IsHalvedProperty = DependencyProperty.Register("IsHalved", typeof(bool), typeof(Ellipse));
		private static DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(TimeSpan), typeof(Ellipse));

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
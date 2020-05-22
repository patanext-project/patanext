using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GameHost.Applications;
using GameHost.Core.Threading;
using GameHost.UI.Noesis;
using Noesis;
using PataponGameHost.Applications.MainThread;
using PataponGameHost.Storage;

namespace PataNext.Module.Presentation.Controls
{
	public class MenuEntryControl : LoadableUserControl<MenuEntryControl.ViewModel>
	{
		public override void OnLoad()
		{
			try
			{
				FindName<Button>("RefreshList").Click += onRefreshList;
				var template = FindName<ItemsControl>("BgmList").ItemTemplate;
				template.FindName<Button>("SelectBgm").Click += onBgmSelected;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			DataContext = new ViewModel
			{
				BgmEntries = new ObservableCollection<BgmEntry>
				{
					new BgmEntry {Content = new BgmDescription {Name = "BGM #1"}},
					new BgmEntry {Content = new BgmDescription {Name = "BGM #2"}}
				}
			};
		}

		public override void OnUnload()
		{
		}

		public override void Dispose()
		{
		}

		private void onRefreshList(object sender, RoutedEventArgs args)
		{
			ThreadingHost.Synchronize<MainThreadHost>(application =>
			{
				application.WorldCollection.Mgr.CreateEntity()
				           .Set(new RefreshBgmList());
			}, null);
		}

		private void onBgmSelected(object sender, RoutedEventArgs args)
		{
			var entry = (BgmEntry) ((Button) sender).Content;
			ThreadingHost.Synchronize<MainThreadHost>(application => { Console.WriteLine("load " + entry.Content.Id); }, null);
		}

		public class ViewModel : NotifyPropertyChangedBase
		{
			private ObservableCollection<BgmEntry> bgmEntries;

			public IEnumerable<BgmEntry> BgmEntries
			{
				get => bgmEntries;
				set
				{
					if (value == null || bgmEntries?.SequenceEqual(value) == true)
						return;

					bgmEntries = new ObservableCollection<BgmEntry>(value);
					OnPropertyChanged(nameof(BgmEntries));
				}
			}
		}

		public class BgmEntry
		{
			public BgmDescription Content { get; set; }

			public override string ToString()
			{
				return Content.Name;
			}
		}
	}
}
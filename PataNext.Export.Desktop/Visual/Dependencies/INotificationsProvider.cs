using System;
using System.Collections.Generic;

namespace PataNext.Export.Desktop.Visual.Dependencies
{
	public interface INotificationsProvider
	{
		IReadOnlyList<Notification> GetAll();

		void Push(Notification notification);

		void ClearAll();
		void Clear(Type type);

		void Remove(Notification notification);

		event Action<Notification> OnNotificationAdded;
		event Action<Notification> OnNotificationRemoved;
	}
}
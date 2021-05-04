using System;
using System.Collections.Generic;

namespace PataNext.Export.Desktop.Visual.Dependencies
{
	public interface IGlobalNotifications
	{
		IReadOnlyList<NotificationBase> GetAll();

		void Push(NotificationBase notification);

		void ClearAll();
		void Clear(Type type);

		void Remove(NotificationBase notification);

		event Action<NotificationBase> OnNotificationAdded;
		event Action<NotificationBase> OnNotificationRemoved;
	}
}
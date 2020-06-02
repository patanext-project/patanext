using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Noesis;

namespace PataNext.Module.Presentation.Controls
{
    public class VisibleIfTrueConverter : IValueConverter
    {
        public static readonly VisibleIfTrueConverter Instance = new VisibleIfTrueConverter();
        
        private static readonly DependencyProperty InstanceProperty = DependencyProperty.Register(
            "Instance", typeof(VisibleIfTrueConverter), typeof(VisibleIfTrueConverter), new PropertyMetadata(new VisibleIfTrueConverter()));
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool flag)
                return flag ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }
}

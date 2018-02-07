using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using Immutable.ProjectModel;

namespace Demo
{
    public partial class MainWindow
    {
        private sealed class FieldKindValueConverter : IValueConverter
        {
            private readonly FieldKind _fieldKind;

            public FieldKindValueConverter(FieldKind fieldKind)
            {
                _fieldKind = fieldKind;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (targetType == typeof(string))
                    return _fieldKind.Format(value);

                return DependencyProperty.UnsetValue;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is string text && _fieldKind.TryParse(text, out var result))
                    return result;

                return DependencyProperty.UnsetValue;
            }
        }
    }
}

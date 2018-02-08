using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using Demo.ViewModels;

using Immutable.ProjectModel;

namespace Demo
{
    public partial class MainWindow
    {
        private sealed class FieldKindValueConverter : IValueConverter
        {
            private readonly ProjectWorkspace _workspace;
            private readonly FieldKind _fieldKind;

            public FieldKindValueConverter(ProjectWorkspace workspace, FieldKind fieldKind)
            {
                _workspace = workspace;
                _fieldKind = fieldKind;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (targetType == typeof(string))
                    return _fieldKind.Format(_workspace.Current, value);

                return DependencyProperty.UnsetValue;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is string text && _fieldKind.TryParse(_workspace.Current, text, out var result))
                    return result;

                return DependencyProperty.UnsetValue;
            }
        }
    }
}

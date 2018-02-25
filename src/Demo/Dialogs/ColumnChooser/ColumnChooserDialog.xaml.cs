using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Immutable.ProjectModel;

namespace Demo.Dialogs.ColumnChooser
{
    internal sealed partial class ColumnChooserDialog : Window
    {
        public ColumnChooserDialog(IEnumerable<FieldDefinition> availableFields, IEnumerable<FieldDefinition> selectedFields, IEnumerable<FieldDefinition> defaultFields)
        {
            InitializeComponent();

            ViewModel = new ColumnChooserViewModel(availableFields, selectedFields, defaultFields);
            DataContext = ViewModel;
        }

        private ColumnChooserViewModel ViewModel { get; }

        public IEnumerable<FieldDefinition> SelectedColumns => ViewModel.SelectedColumns.Select(c => c.Field);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AvailableTextBox.Focus();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Add();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Remove();
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MoveUp();
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MoveDown();
        }

        private void AvailableListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.Add();
        }

        private void SelectedListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.Remove();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Reset();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}

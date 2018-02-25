using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

using Demo.ViewModels;

using Immutable.ProjectModel;

namespace Demo.Dialogs.ColumnChooser
{
    internal sealed class ColumnChooserViewModel : ViewModel
    {
        private string _selectedFilter;
        private string _availableFilter;

        public ColumnChooserViewModel(IEnumerable<FieldDefinition> availableFields, IEnumerable<FieldDefinition> selectedFields, IEnumerable<FieldDefinition> defaultFields)
        {
            AvailableColumns = new ObservableCollection<ColumnViewModel>(availableFields.Select(f => new ColumnViewModel(f)));
            SelectedColumns = new ObservableCollection<ColumnViewModel>();

            var columns = AvailableColumns.ToDictionary(c => c.Field);

            foreach (var selectedField in selectedFields)
            {
                if (columns.TryGetValue(selectedField, out var column))
                {
                    column.IsSelected = true;
                    Add();
                }
            }

            var availableView = CollectionViewSource.GetDefaultView(AvailableColumns);
            availableView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            DefaultColumns = new ReadOnlyCollection<ColumnViewModel>(defaultFields.Select(f => columns[f]).ToArray());
        }

        public void Add()
        {
            var selected = AvailableColumns.Where(c => c.IsSelected).ToArray();

            foreach (var item in selected)
            {
                AvailableColumns.Remove(item);
                item.IsSelected = false;
                SelectedColumns.Add(item);
            }
        }

        public void Remove()
        {
            var selected = SelectedColumns.Where(c => c.IsSelected).ToArray();

            foreach (var item in selected)
            {
                SelectedColumns.Remove(item);
                item.IsSelected = false;
                AvailableColumns.Add(item);
            }
        }

        public void MoveUp()
        {
            if (SelectedColumns.Count == 0 || SelectedColumns.First().IsSelected)
                return;

            var selected = SelectedColumns.Where(c => c.IsSelected).ToArray();

            foreach (var item in selected)
            {
                var index = SelectedColumns.IndexOf(item);
                SelectedColumns.RemoveAt(index);
                SelectedColumns.Insert(index - 1, item);
            }
        }

        public void MoveDown()
        {
            if (SelectedColumns.Count == 0 || SelectedColumns.Last().IsSelected)
                return;

            var selected = SelectedColumns.Where(c => c.IsSelected).Reverse().ToArray();

            foreach (var item in selected)
            {
                var index = SelectedColumns.IndexOf(item);
                SelectedColumns.RemoveAt(index);
                SelectedColumns.Insert(index + 1, item);
            }
        }

        public void Reset()
        {
            while (SelectedColumns.Count > 0)
            {
                AvailableColumns.Add(SelectedColumns[0]);
                SelectedColumns.RemoveAt(0);
            }

            foreach (var column in DefaultColumns)
            {
                AvailableColumns.Remove(column);
                SelectedColumns.Add(column);
            }
        }

        public string AvailableFilter
        {
            get => _availableFilter;
            set
            {
                if (_availableFilter != value)
                {
                    _availableFilter = value;

                    var view = CollectionViewSource.GetDefaultView(AvailableColumns);
                    if (_availableFilter.Length == 0)
                        view.Filter = null;
                    else
                        view.Filter = o => o is ColumnViewModel vm && vm.Field.Name.IndexOf(_availableFilter, StringComparison.OrdinalIgnoreCase) >= 0;

                    OnPropertyChanged();
                }
            }
        }

        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (_selectedFilter != value)
                {
                    _selectedFilter = value;

                    var view = CollectionViewSource.GetDefaultView(SelectedColumns);
                    if (_selectedFilter.Length == 0)
                        view.Filter = null;
                    else
                        view.Filter = o => o is ColumnViewModel vm && vm.Field.Name.IndexOf(_selectedFilter, StringComparison.OrdinalIgnoreCase) >= 0;

                    OnPropertyChanged();
                }
            }
        }

        public ReadOnlyCollection<ColumnViewModel> DefaultColumns { get; }

        public ObservableCollection<ColumnViewModel> AvailableColumns { get; }

        public ObservableCollection<ColumnViewModel> SelectedColumns { get; }
    }
}

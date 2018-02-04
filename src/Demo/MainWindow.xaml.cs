using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

using Demo.ViewModels;

using Immutable.ProjectModel;

namespace Demo
{
    public partial class MainWindow : Window
    {
        private List<TaskField> _taskFields = new List<TaskField>();
        private List<AssignmentField> _assignmentFields = new List<AssignmentField>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var workspace = new ProjectWorkspace(Project.Create());
            workspace.CurrentChanged += Workspace_CurrentChanged;

            GanttControl.DataContext = new GanttViewModel(workspace);

            foreach (var field in TaskFields.All)
            {
                if (typeof(IList).IsAssignableFrom(field.Type))
                    continue;

                var index = TaskFields.All.IndexOf(field);
                var name = $"Item[{index}]";
                var column = CreateColumn(name, field.Name, field);
                TaskDataGrid.Columns.Add(column);
                _taskFields.Add(field);
            }

            TaskDataGrid.DataContext = new TaskGridViewModel(workspace);

            foreach (var field in AssignmentFields.All)
            {
                var index = AssignmentFields.All.IndexOf(field);
                var name = $"Item[{index}]";
                var column = CreateColumn(name, field.Name, field);
                AssignmentDataGrid.Columns.Add(column);
                _assignmentFields.Add(field);
            }

            AssignmentDataGrid.DataContext = new AssignmentGridViewModel(workspace);

            workspace.ApplyChanges(CreateProject());
        }

        private static DataGridColumn CreateColumn(string name, string header, FieldDefinition field)
        {
            var mode = field.IsReadOnly ? BindingMode.OneTime : BindingMode.TwoWay;

            var column = new DataGridTextColumn
            {
                Binding = new Binding(name)
                {
                    Converter = new FieldKindValueConverter(field.Kind)
                },
                Header = header,
                IsReadOnly = field.IsReadOnly
            };

            return column;
        }

        private static Project CreateProject()
        {
            var designTaskId = TaskId.Create();
            var implementationTaskId = TaskId.Create();
            var migrationTaskId = TaskId.Create();
            var finishTaskId = TaskId.Create();
            var immoResourceId = ResourceId.Create();
            var thomasResourceId = ResourceId.Create();

            return Project.Create()
                          .WithName("Some Software Project")
                          .WithStartDate(new DateTimeOffset(2018, 1, 29, 0, 0, 0, DateTimeOffset.Now.Offset))
                          .AddNewTask(designTaskId)
                              .WithName("Design")
                              .WithDuration(TimeSpan.FromDays(5)).Project
                          .AddNewTask(implementationTaskId)
                              .WithName("Implementation")
                              .WithDuration(TimeSpan.FromDays(10))
                              .AddPredecessorId(designTaskId).Project
                          .AddNewTask(migrationTaskId)
                              .WithName("Migration")
                              .WithDuration(TimeSpan.FromDays(8)).Project
                          .AddNewTask(finishTaskId)
                              .WithName("Finish")
                              .AddPredecessorId(implementationTaskId)
                              .AddPredecessorId(migrationTaskId).Project
                          .AddNewResource(immoResourceId)
                              .WithName("Immo").Project
                          .AddNewResource(thomasResourceId)
                              .WithName("Thomas").Project
                          .AddNewAssignment(designTaskId, immoResourceId).Project
                          .AddNewAssignment(implementationTaskId, immoResourceId).Project
                          .AddNewAssignment(implementationTaskId, thomasResourceId).Project
                          .AddNewAssignment(migrationTaskId, thomasResourceId).Project;
        }

        private void Workspace_CurrentChanged(object sender, ProjectChangedEventArgs e)
        {
            Title = e.NewProject.Name;
            HighlightChangedCells(TaskDataGrid, e.Changes.ChangedTasks);
            HighlightChangedCells(AssignmentDataGrid, e.Changes.ChangedAssignments);
        }

        private void HighlightChangedCells(DataGrid dataGrid, ImmutableArray<TaskChanges> changedTasks)
        {
            var viewModel = dataGrid.DataContext as TaskGridViewModel;
            if (viewModel == null)
                return;

            var changesById = changedTasks.ToDictionary(c => c.Id, c => new HashSet<TaskField>(c.FieldChanges.Select(fc => fc.Field)));
            var empty = new HashSet<TaskField>();

            for (var rowIndex = 0; rowIndex < viewModel.Rows.Count; rowIndex++)
            {
                var taskId = viewModel.Rows[rowIndex].TaskId;
                if (!changesById.TryGetValue(taskId, out var changedFields))
                    changedFields = empty;

                for (var columnIndex = 0; columnIndex < _taskFields.Count; columnIndex++)
                {
                    var field = _taskFields[columnIndex];
                    var cell = GetCell(dataGrid, rowIndex, columnIndex);
                    if (cell != null)
                        HighlightCell(cell, changedFields.Contains(field));
                }
            }
        }

        private void HighlightChangedCells(DataGrid dataGrid, ImmutableArray<AssignmentChanges> changedAssignments)
        {
            var viewModel = dataGrid.DataContext as AssignmentGridViewModel;
            if (viewModel == null)
                return;

            var changesById = changedAssignments.ToDictionary(c => c.Id, c => new HashSet<AssignmentField>(c.FieldChanges.Select(fc => fc.Field)));
            var empty = new HashSet<AssignmentField>();

            for (var rowIndex = 0; rowIndex < viewModel.Rows.Count; rowIndex++)
            {
                var assignmentId = viewModel.Rows[rowIndex].AssignmentId;
                if (!changesById.TryGetValue(assignmentId, out var changedFields))
                    changedFields = empty;

                for (var columnIndex = 0; columnIndex < _assignmentFields.Count; columnIndex++)
                {
                    var field = _assignmentFields[columnIndex];
                    var cell = GetCell(dataGrid, rowIndex, columnIndex);
                    if (cell != null)
                        HighlightCell(cell, changedFields.Contains(field));
                }
            }
        }

        private static void HighlightCell(DataGridCell cell, bool isHighlighted)
        {
            if (isHighlighted)
            {
                cell.Background = Brushes.LightBlue;
                cell.Foreground = Brushes.Black;
            }
            else
            {
                cell.ClearValue(BackgroundProperty);
                cell.ClearValue(ForegroundProperty);
            }
        }

        private static DataGridCell GetCell(DataGrid dataGrid, int rowIndex, int columnIndex)
        {
            var row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            var p = GetVisualChild<DataGridCellsPresenter>(row);
            return p.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
        }

        private static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            var child = default(T);
            var numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < numVisuals; i++)
            {
                var v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                    child = GetVisualChild<T>(v);
                if (child != null)
                    break;
            }
            return child;
        }

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

                return null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is string text && _fieldKind.TryParse(text, out var result))
                    return result;

                return null;
            }
        }
    }
}

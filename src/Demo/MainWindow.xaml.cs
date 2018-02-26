using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

using Demo.Dialogs.ColumnChooser;
using Demo.Services;
using Demo.ViewModels;

using Immutable.ProjectModel;

namespace Demo
{
    [Export]
    internal sealed partial class MainWindow : Window
    {
        private readonly WorkspaceService _workspace;
        private readonly CommandService _commandService;
        private readonly UndoRedoService _undoRedoService;
        private readonly List<TaskField> _taskFields = new List<TaskField>(TaskFields.Default);
        private readonly List<ResourceField> _resourceFields = new List<ResourceField>(ResourceFields.Default);
        private readonly List<AssignmentField> _assignmentFields = new List<AssignmentField>(AssignmentFields.Default);

        [ImportingConstructor]
        public MainWindow(WorkspaceService workspace, CommandService commandService, UndoRedoService undoRedoService)
        {
            InitializeComponent();

            _workspace = workspace;
            _workspace.CurrentChanged += Workspace_CurrentChanged;
            _commandService = commandService;
            _undoRedoService = undoRedoService;
        }

        public IEnumerable<TaskField> SelectedTaskFields => _taskFields;

        public IEnumerable<ResourceField> SelectedResourceFields => _resourceFields;

        public IEnumerable<AssignmentField> SelectedAssignmentFields => _assignmentFields;

        private static Project CreateProject()
        {
            var time = TimeConversion.Default;
            var designTaskId = TaskId.Create();
            var implementationTaskId = TaskId.Create();
            var migrationTaskId = TaskId.Create();
            var finishTaskId = TaskId.Create();
            var immoResourceId = ResourceId.Create();
            var thomasResourceId = ResourceId.Create();

            return Project.Create()
                          .WithName("Some Software Project")
                          .WithStart(new DateTimeOffset(2018, 1, 29, 0, 0, 0, DateTimeOffset.Now.Offset))
                          .AddTask(designTaskId)
                              .WithName("Design")
                              .WithDuration(time.FromDays(5)).Project
                          .AddTask(implementationTaskId)
                              .WithName("Implementation")
                              .WithDuration(time.FromDays(10))
                              .AddPredecessorLink(designTaskId).Project
                          .AddTask(migrationTaskId)
                              .WithName("Migration")
                              .WithDuration(time.FromDays(8)).Project
                          .AddTask(finishTaskId)
                              .WithName("Finish")
                              .AddPredecessorLink(implementationTaskId)
                              .AddPredecessorLink(migrationTaskId).Project
                          .AddResource(immoResourceId)
                              .WithName("Immo").Project
                          .AddResource(thomasResourceId)
                              .WithName("Thomas").Project
                          .AddAssignment(designTaskId, immoResourceId).Project
                          .AddAssignment(implementationTaskId, immoResourceId).Project
                          .AddAssignment(implementationTaskId, thomasResourceId).Project
                          .AddAssignment(migrationTaskId, thomasResourceId).Project;
        }

        private DataGridColumn CreateColumn(string name, string header, FieldDefinition field)
        {
            var mode = field.IsReadOnly ? BindingMode.OneTime : BindingMode.TwoWay;

            if (field.Kind.HasSuggestions)
            {
                var formattedItems = field.Kind.GetSuggestions()
                                               .OfType<object>()
                                               .Select(o => new { Value = o, Text = field.Kind.Format(_workspace.Current, o) });

                return new DataGridComboBoxColumn
                {
                    SelectedValueBinding = new Binding(name),
                    SelectedValuePath = "Value",
                    DisplayMemberPath = "Text",
                    ItemsSource = formattedItems,
                    Header = header,
                    IsReadOnly = field.IsReadOnly
                };
            }
            else
            {
                return new DataGridTextColumn
                {
                    Binding = new Binding(name)
                    {
                        Converter = new FieldKindValueConverter(_workspace, field.Kind)
                    },
                    Header = header,
                    IsReadOnly = field.IsReadOnly
                };
            }
        }

        private void UpdateTaskGridColumns()
        {
            TaskDataGrid.Columns.Clear();

            foreach (var field in _taskFields)
            {
                var index = TaskFields.All.IndexOf(field);
                var name = $"Item[{index}]";
                var column = CreateColumn(name, field.Name, field);
                TaskDataGrid.Columns.Add(column);
            }
        }

        private void UpdateResourceGridColumns()
        {
            ResourceDataGrid.Columns.Clear();

            foreach (var field in _resourceFields)
            {
                var index = ResourceFields.All.IndexOf(field);
                var name = $"Item[{index}]";
                var column = CreateColumn(name, field.Name, field);
                ResourceDataGrid.Columns.Add(column);
            }
        }

        private void UpdateAssignmentGridColumns()
        {
            AssignmentDataGrid.Columns.Clear();

            foreach (var field in _assignmentFields)
            {
                var index = AssignmentFields.All.IndexOf(field);
                var name = $"Item[{index}]";
                var column = CreateColumn(name, field.Name, field);
                AssignmentDataGrid.Columns.Add(column);
            }
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

        private void HighlightChangedCells(DataGrid dataGrid, ImmutableArray<ResourceChanges> changedResources)
        {
            var viewModel = dataGrid.DataContext as ResourceGridViewModel;
            if (viewModel == null)
                return;

            var changesById = changedResources.ToDictionary(c => c.Id, c => new HashSet<ResourceField>(c.FieldChanges.Select(fc => fc.Field)));
            var empty = new HashSet<ResourceField>();

            for (var rowIndex = 0; rowIndex < viewModel.Rows.Count; rowIndex++)
            {
                var resourceId = viewModel.Rows[rowIndex].ResourceId;
                if (!changesById.TryGetValue(resourceId, out var changedFields))
                    changedFields = empty;

                for (var columnIndex = 0; columnIndex < _resourceFields.Count; columnIndex++)
                {
                    var field = _resourceFields[columnIndex];
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _commandService;

            GanttControl.DataContext = new GanttViewModel(_workspace);

            UpdateTaskGridColumns();
            TaskDataGrid.DataContext = new TaskGridViewModel(_workspace);

            UpdateResourceGridColumns();
            ResourceDataGrid.DataContext = new ResourceGridViewModel(_workspace);

            UpdateAssignmentGridColumns();
            AssignmentDataGrid.DataContext = new AssignmentGridViewModel(_workspace);

            _workspace.ApplyChanges(CreateProject());
            _undoRedoService.Reset();
        }

        private void Workspace_CurrentChanged(object sender, ProjectChangedEventArgs e)
        {
            Title = e.NewProject.Name;
            HighlightChangedCells(TaskDataGrid, e.Changes.ChangedTasks);
            HighlightChangedCells(ResourceDataGrid, e.Changes.ChangedResources);
            HighlightChangedCells(AssignmentDataGrid, e.Changes.ChangedAssignments);
        }

        private void ViewSetScheduleColumnsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _taskFields.Clear();
            _taskFields.AddRange(new TaskField[] {
                TaskFields.Ordinal,
                TaskFields.Name,
                TaskFields.Duration,
                TaskFields.Work,
                TaskFields.Start,
                TaskFields.Finish,
                TaskFields.EarlyStart,
                TaskFields.EarlyFinish,
                TaskFields.LateStart,
                TaskFields.LateFinish,
                TaskFields.StartSlack,
                TaskFields.FinishSlack,
                TaskFields.TotalSlack,
                TaskFields.FreeSlack,
                TaskFields.IsCritical,
            });
            UpdateTaskGridColumns();
        }

        private void TaskGridAddButton_Click(object sender, RoutedEventArgs e)
        {
            var project = _workspace.Current.AddTask().Project;
            _workspace.ApplyChanges(project);
        }

        private void TaskGridRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var project = _workspace.Current;

            foreach (var cell in TaskDataGrid.SelectedCells)
            {
                if (cell.Item is TaskGridRowViewModel row)
                    project = project.RemoveTask(row.TaskId);
            }

            _workspace.ApplyChanges(project);
        }

        private void TaskGridSelectColumnsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColumnChooserDialog(TaskFields.All, _taskFields, TaskFields.Default)
            {
                Owner = this
            };
            if (dialog.ShowDialog() == true)
            {
                _taskFields.Clear();
                _taskFields.AddRange(dialog.SelectedColumns.OfType<TaskField>());
                UpdateTaskGridColumns();
            }
        }

        private void ResourceGridAddButton_Click(object sender, RoutedEventArgs e)
        {
            var project = _workspace.Current.AddResource().Project;
            _workspace.ApplyChanges(project);
        }

        private void ResourceGridRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var project = _workspace.Current;

            foreach (var cell in ResourceDataGrid.SelectedCells)
            {
                if (cell.Item is ResourceGridRowViewModel row)
                    project = project.RemoveResource(row.ResourceId);
            }

            _workspace.ApplyChanges(project);
        }

        private void ResourceGridSelectColumnsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColumnChooserDialog(ResourceFields.All, _resourceFields, ResourceFields.Default)
            {
                Owner = this
            };
            if (dialog.ShowDialog() == true)
            {
                _resourceFields.Clear();
                _resourceFields.AddRange(dialog.SelectedColumns.OfType<ResourceField>());
                UpdateResourceGridColumns();
            }
        }

        private void AssignmentGridAddButton_Click(object sender, RoutedEventArgs e)
        {
            var project = _workspace.Current;

            var selectedTasks = TaskDataGrid.SelectedCells.Select(c => c.Item).OfType<TaskGridRowViewModel>().Distinct().ToArray();
            var selectedResources = ResourceDataGrid.SelectedCells.Select(c => c.Item).OfType<ResourceGridRowViewModel>().Distinct().ToArray();

            foreach (var task in selectedTasks)
            {
                foreach (var resource in selectedResources)
                {
                    if (project.GetAssignment(task.TaskId, resource.ResourceId) == null)
                        project = project.AddAssignment(task.TaskId, resource.ResourceId).Project;
                }
            }

            _workspace.ApplyChanges(project);
        }

        private void AssignmentGridRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var project = _workspace.Current;

            foreach (var cell in AssignmentDataGrid.SelectedCells)
            {
                if (cell.Item is AssignmentGridRowViewModel row)
                    project = project.RemoveAssignment(row.AssignmentId);
            }

            _workspace.ApplyChanges(project);
        }

        private void AssignmentGridSelectColumnsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColumnChooserDialog(AssignmentFields.All, _assignmentFields, AssignmentFields.Default)
            {
                Owner = this
            };
            if (dialog.ShowDialog() == true)
            {
                _assignmentFields.Clear();
                _assignmentFields.AddRange(dialog.SelectedColumns.OfType<AssignmentField>());
                UpdateAssignmentGridColumns();
            }
        }
    }
}

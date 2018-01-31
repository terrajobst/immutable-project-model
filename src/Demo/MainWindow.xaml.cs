﻿using System;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var project = CreateProject();
            var workspace = new ProjectWorkspace(project);
            workspace.CurrentChanged += Workspace_CurrentChanged;

            GanttControl.DataContext = new GanttViewModel(workspace);

            foreach (var field in TaskFields.All)
            {
                if (typeof(IList).IsAssignableFrom(field.Type))
                    continue;

                var index = TaskFields.All.IndexOf(field);
                var name = $"Item[{index}]";
                var header = field.Name.Replace("Task.", string.Empty);
                var column = CreateColumn(name, header, field);
                TaskDataGrid.Columns.Add(column);
            }

            TaskDataGrid.DataContext = new TaskGridViewModel(workspace);

            foreach (var field in AssignmentFields.All)
            {
                var index = AssignmentFields.All.IndexOf(field);
                var name = $"Item[{index}]";
                var header = field.Name.Replace("Assignment.", string.Empty);
                var column = CreateColumn(name, header, field);
                AssignmentDataGrid.Columns.Add(column);
            }

            AssignmentDataGrid.DataContext = new AssignmentGridViewModel(workspace);
        }

        private static DataGridColumn CreateColumn(string name, string header, FieldDefinition field)
        {
            var mode = field.IsReadOnly ? BindingMode.OneTime : BindingMode.TwoWay;

            if (field.Type == typeof(bool))
            {
                return new DataGridCheckBoxColumn()
                {
                    Binding = new Binding(name),
                    Header = header,
                    IsReadOnly = field.IsReadOnly
                };
            }

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

            return Project.Empty
                          .AddNewTask(designTaskId)
                              .WithName("Design")
                              .WithDuration(TimeSpan.FromDays(5)).Project
                          .AddNewTask(implementationTaskId)
                              .WithName("Implementation")
                              .WithDuration(TimeSpan.FromDays(10))
                              .WithWork(TimeSpan.FromHours(40))
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
            HighlightChangedCells(TaskDataGrid, e.Changes.ChangedTasks);
            HighlightChangedCells(AssignmentDataGrid, e.Changes.ChangedAssignments);
        }

        private static void HighlightChangedCells(DataGrid dataGrid, ImmutableArray<TaskChanges> changedTasks)
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

                for (var columnIndex = 0; columnIndex < TaskFields.All.Length; columnIndex++)
                {
                    var field = TaskFields.All[columnIndex];
                    var cell = GetCell(dataGrid, rowIndex, columnIndex);
                    if (cell != null)
                    {
                        if (changedFields.Contains(field))
                            cell.Background = Brushes.LightBlue;
                        else
                            cell.ClearValue(BackgroundProperty);
                    }
                }
            }
        }

        private static void HighlightChangedCells(DataGrid dataGrid, ImmutableArray<AssignmentChanges> changedAssignments)
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

                for (var columnIndex = 0; columnIndex < AssignmentFields.All.Length; columnIndex++)
                {
                    var field = AssignmentFields.All[columnIndex];
                    var cell = GetCell(dataGrid, rowIndex, columnIndex);
                    if (cell != null)
                    {
                        if (changedFields.Contains(field))
                            cell.Background = Brushes.LightBlue;
                        else
                            cell.ClearValue(BackgroundProperty);
                    }
                }
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
